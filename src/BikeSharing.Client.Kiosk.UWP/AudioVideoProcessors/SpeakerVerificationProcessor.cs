using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.SpeakerRecognition;
using Microsoft.ProjectOxford.SpeakerRecognition.Contract.Verification;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using System.Diagnostics;

namespace BikeSharing.Client.Kiosk.UWP.AudioVideoProcessors
{
    public class SpeakerVerificationProcessor
    {
        private MediaCapture _audioCapture;
        private InMemoryRandomAccessStream _buffer;
        private bool _isRecording;
        private SpeakerVerificationServiceClient _speakerVerificationServiceClient;
        private TaskCompletionSource<bool> _recordingTaskCompletionSource;

        public SpeakerVerificationProcessor()
        {
            _speakerVerificationServiceClient = new SpeakerVerificationServiceClient(App.COGNITIVE_SERVICES_SPEAKER_API_SUBSCRIPTION_KEY);
        }

        public async Task InitializeAsync()
        {
            this.CleanupAsync();
            try
            {
                MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Audio
                };
                _audioCapture = new MediaCapture();
                await _audioCapture.InitializeAsync(settings);
                _audioCapture.RecordLimitationExceeded += async (_) =>
                {
                    await StopAsync();
                    throw new Exception("Exceeded Recording limitation");
                };
                _audioCapture.Failed += (_, errorEventArgs) =>
                {
                    _isRecording = false;
                    //throw new Exception($"Recoding failed: {errorEventArgs.Code} {errorEventArgs.Message}");
                };
                _buffer = new InMemoryRandomAccessStream();
            }
            catch (Exception ex) when (ex.InnerException != null && ex.InnerException is UnauthorizedAccessException)
            {
                throw ex.InnerException;
            }
        }

        public async Task<bool> VerifyAsync(Guid speakerProfileId, TimeSpan timeout)
        {
            if (_audioCapture == null)
            {
                await InitializeAsync();
            }

            try
            {
                // Maximum allowed length by the Speaker Recognition Api is 15 seconds.
                if (timeout > TimeSpan.FromSeconds(15))
                    timeout = TimeSpan.FromSeconds(15);

                var timeoutTask = Task.Delay(timeout).ContinueWith(async (a) =>
                {
                    await this.StopAsync();
                });
                var recodingTask = this.StartRecordingAsync();
                await Task.WhenAll(timeoutTask, recodingTask);
                return await this.VerifySpeackerAsync(speakerProfileId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Speaker verification]: an error occured: {ex.ToString()} ");
                return false;
            }
        }

        public async Task StartRecordingAsync()
        {
            if (!_isRecording)
            {
                _isRecording = true;
                var outProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Medium);
                outProfile.Audio = AudioEncodingProperties.CreatePcm(16000, 1, 16);
                _recordingTaskCompletionSource = new TaskCompletionSource<bool>();
                await _audioCapture.StartRecordToStreamAsync(outProfile, _buffer);
                await _recordingTaskCompletionSource.Task;
            }
        }

        private async Task StopAsync()
        {
            await _audioCapture.StopRecordAsync();
            if (_recordingTaskCompletionSource != null && !_recordingTaskCompletionSource.Task.IsCompleted)
            {
                _recordingTaskCompletionSource.SetResult(true);
            }
            _isRecording = false;
        }

        private async Task<bool> VerifySpeackerAsync(Guid speakerProfileId)
        {
            if (_buffer != null)
            {
                IRandomAccessStream audio = null;
                try
                {
                    audio = _buffer.CloneStream();
                    var fixedStream = FixWavPcmStream(audio);
                    var response = await _speakerVerificationServiceClient.VerifyAsync(fixedStream, speakerProfileId);
                    return (response.Result == Result.Accept);
                }
                finally
                {
                    audio.Dispose();
                }
            }

            return false;
        }

        // WAV catpured by UWP MediaCapture is not recognized by the Speaker Recognition service.
        // Applying the fix from
        // https://mtaulty.com/2016/02/10/project-oxfordspeaker-verification-from-a-windows-10uwp-app/
        private Stream FixWavPcmStream(IInputStream inputStream)
        {
            var netStream = inputStream.AsStreamForRead();
            var bits = new byte[netStream.Length];
            netStream.Read(bits, 0, bits.Length);

            var pcmFileLength = BitConverter.ToInt32(bits, 4);

            pcmFileLength -= 36;

            for (int i = 0; i < 12; i++)
            {
                bits[i + 36] = bits[i];
            }

            var newLengthBits = BitConverter.GetBytes(pcmFileLength);
            newLengthBits.CopyTo(bits, 40);

            MemoryStream stream = new MemoryStream(bits, 36, bits.Length - 36);
            return stream;
        }


        private void CleanupAsync()
        {
            if (_audioCapture != null)
            {
                _audioCapture.Dispose();
                _audioCapture = null;
            }
            if (_buffer != null)
            {
                _buffer.Dispose();
                _buffer = null;
            }
        }
    }

}

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

namespace BikeSharing.Clients.CogServicesKiosk.Data
{
    public class AudioRecorder
    {

        public AudioRecorder()
        {
        }

        public async Task<Stream> RecordAsync(TimeSpan timeToRecord)
        {
            MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Audio
            };

            MediaCapture audioCapture = new MediaCapture();
            await audioCapture.InitializeAsync(settings);
            //_audioCapture.RecordLimitationExceeded += (_) =>
            //{
            //    //await StopAsync();
            //    throw new Exception("Exceeded Recording limitation");
            //};
            //_audioCapture.Failed += (_, errorEventArgs) =>
            //{
            //    //_isRecording = false;
            //    //throw new Exception($"Recoding failed: {errorEventArgs.Code} {errorEventArgs.Message}");
            //};

            var outProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Medium);
            outProfile.Audio = AudioEncodingProperties.CreatePcm(16000, 1, 16);

            var buffer = new InMemoryRandomAccessStream();
            await audioCapture.StartRecordToStreamAsync(outProfile, buffer);
            await Task.Delay(timeToRecord);
            await audioCapture.StopRecordAsync();
            
            IRandomAccessStream audio = null;
            try
            {
                audio = buffer.CloneStream();
                return this.FixWavPcmStream(audio);
            }
            finally
            {
                audio.Dispose();
            }
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
                bits[i + 36] = bits[i];

            var newLengthBits = BitConverter.GetBytes(pcmFileLength);
            newLengthBits.CopyTo(bits, 40);

            MemoryStream stream = new MemoryStream(bits, 36, bits.Length - 36);
            return stream;
        }
    }
}

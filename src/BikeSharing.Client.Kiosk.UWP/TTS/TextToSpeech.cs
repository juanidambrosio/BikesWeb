using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TTSSample;

namespace BikeSharing.Client.Kiosk.UWP.TTS
{
    public class TextToSpeech: IDisposable
    {
        private const string TemporyAudioFileName = "tts.wav";

        private const string requestUri = "https://speech.platform.bing.com/synthesize";

        Synthesize _synthesizer;
        AudioGraphWrapper _audioGraph;
        TaskCompletionSource<bool> _playAudioTaskCompletionSource;

        public TextToSpeech()
        {
        }

        public async Task InitializeSynthesizerAsync()
        {
            Debug.WriteLine("[Text to Speech]: Starting authtentication for Speech Api subscription");
            string accessToken;

            // Note: The way to get api key:
            // Free: https://www.microsoft.com/cognitive-services/en-us/subscriptions?productId=/products/Bing.Speech.Preview
            // Paid: https://portal.azure.com/#create/Microsoft.CognitiveServices/apitype/Bing.Speech/pricingtier/S0
            Authentication auth = new Authentication(App.COGNITIVE_SERVICES_BING_SPEECH_SUBSCRIPTION_KEY);

            try
            {
                accessToken = await auth.GetAccessTokenAsync();
                Debug.WriteLine("[Text to Speech]: Token: {0}\n", accessToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Text to Speech]: Failed authentication.");
                Debug.WriteLine(ex.ToString());
                Debug.WriteLine(ex.Message);
                return;
            }

            Debug.WriteLine("[Text to Speech]: Starting request code execution.");

            _synthesizer = new Synthesize(new Synthesize.InputOptions()
            {
                RequestUri = new Uri(requestUri),
                // Text to be spoken.
                Text = "Hi, how are you doing?",
                VoiceType = Gender.Female,
                // Refer to the documentation for complete list of supported locales.
                Locale = "en-US",
                // You can also customize the output voice. Refer to the documentation to view the different
                // voices that the TTS service can output.
                VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)",
                // Service can return audio in different output format.
                OutputFormat = AudioOutputFormat.Riff16Khz16BitMonoPcm,
                AuthorizationToken = "Bearer " + accessToken,
            });

            _synthesizer.OnAudioAvailable += PlayAudio;
            _synthesizer.OnError += ErrorHandler;

            _audioGraph = new AudioGraphWrapper();
            await _audioGraph.CreateAudioGraph();
            _audioGraph.AudioFileCompleted += AudioGraph_AudioFileCompleted;
        }

        private void AudioGraph_AudioFileCompleted(object sender, EventArgs e)
        {
            _audioGraph.Stop();
            _playAudioTaskCompletionSource.SetResult(true);
        }

        public async Task SpeakAsync(string text)
        {
            _playAudioTaskCompletionSource = new TaskCompletionSource<bool>();
            Debug.WriteLine($"[Text to Speech]: Speaking: \"{text}\"");
            await _synthesizer.Speak(text, CancellationToken.None);
            await _playAudioTaskCompletionSource.Task;
        }

        private async void PlayAudio(object sender, GenericEventArgs<Stream> e)
        {
            if (_playAudioTaskCompletionSource != null &&
                !_playAudioTaskCompletionSource.Task.IsCompleted)
            {
                try
                {
                    var stream = new MemoryStream();
                    e.EventData.CopyTo(stream);
                    await SaveWaveStreamToFile(stream);

                    var file = await Windows.Storage.KnownFolders.MusicLibrary.GetFileAsync(TemporyAudioFileName);
                    _audioGraph.Stop();
                    if (await _audioGraph.SetAudioFileSource(file))
                    {
                        _audioGraph.Play();
                        await _playAudioTaskCompletionSource.Task;
                    }
                    else
                    {
                        _playAudioTaskCompletionSource.SetResult(false);
                    }

                    await file.DeleteAsync(Windows.Storage.StorageDeleteOption.PermanentDelete);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Text to Speech]: an error occurred when playing audio: {ex.ToString()}");
                }
                finally
                {
                    e.EventData.Dispose();
                }
            }
        }

        private void ErrorHandler(object sender, GenericEventArgs<Exception> e)
        {
            Debug.WriteLine($"[Text to Speech]: an error occured when synthesizing: {(e.EventData as Exception)?.ToString()}.");
            _audioGraph.Stop();
            _playAudioTaskCompletionSource.SetResult(true);
        }

        private async Task SaveWaveStreamToFile(Stream stream)
        {
            if (stream == null)
            {
                Debug.WriteLine("[Text to Speech]: No Speech data available to save.");
                return;
            }

            stream.Position = 0;

            var file = await Windows.Storage.KnownFolders.MusicLibrary.CreateFileAsync(TemporyAudioFileName,
                Windows.Storage.CreationCollisionOption.ReplaceExisting);

            if (file != null)
            {
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                var outStream = await file.OpenStreamForWriteAsync();
                await stream.CopyToAsync(outStream);
                await outStream.FlushAsync();
                outStream.Dispose();
            }
        }

        private void Cleanup()
        {
            if (_synthesizer != null)
            {
                _synthesizer.OnAudioAvailable -= PlayAudio;
                _synthesizer.OnError -= ErrorHandler;
                _synthesizer = null;

                _audioGraph.AudioFileCompleted -= AudioGraph_AudioFileCompleted;
                _audioGraph.Cleanup();
                _audioGraph = null;
            }
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}

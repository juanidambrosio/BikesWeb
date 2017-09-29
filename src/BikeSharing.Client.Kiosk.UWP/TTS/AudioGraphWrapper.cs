using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.Storage;

namespace BikeSharing.Client.Kiosk.UWP.TTS
{
    public class AudioGraphWrapper
    {
        private AudioGraph _graph;
        private AudioFileInputNode _fileInput;
        private AudioDeviceOutputNode _deviceOutput;

        public event EventHandler AudioFileCompleted;

        public async Task CreateAudioGraph()
        {
            var settings = new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.Media);
            var result = await AudioGraph.CreateAsync(settings);
            if (result.Status != AudioGraphCreationStatus.Success)
            {
                Debug.WriteLine($"[Text to Speech]: AudioGraph Creation Error because {result.Status.ToString()}");
                return;
            }

            _graph = result.Graph;

            var deviceOutputNodeResult = await _graph.CreateDeviceOutputNodeAsync();
            if (deviceOutputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                Debug.WriteLine($"[Text to Speech]: Device Output unavailable because {deviceOutputNodeResult.Status.ToString()}");
                return;
            }

            _deviceOutput = deviceOutputNodeResult.DeviceOutputNode;
            Debug.WriteLine("[Text to Speech]: Device Output Node successfully created");
        }

        public async Task<bool> SetAudioFileSource(IStorageFile file)
        {
            var fileInputResult = await _graph.CreateFileInputNodeAsync(file);
            if (fileInputResult.Status != AudioFileNodeCreationStatus.Success)
            {
                Debug.WriteLine($"[Text to Speech]: Cannot read input file because {fileInputResult.Status.ToString()}");
                return false;
            }

            CleanupFileInput();
            _fileInput = fileInputResult.FileInputNode;
            _fileInput.AddOutgoingConnection(_deviceOutput);
            _fileInput.FileCompleted += FileInput_FileCompleted;
            return true;
        }

        private void FileInput_FileCompleted(AudioFileInputNode sender, object args)
        {
            this.AudioFileCompleted?.Invoke(sender, args as EventArgs);
        }

        public void Play()
        {
            _graph.Start();
        }

        public void Stop()
        {
            _graph.Stop();
        }

        public void Cleanup()
        {
            if (_graph != null)
            {
                _graph.Stop();
            }

            CleanupFileInput();

            if (_deviceOutput != null)
            {
                _deviceOutput.Dispose();
            }

            if (_graph != null)
            {
                _graph.Dispose();
            }
        }

        private void CleanupFileInput()
        {
            if (_fileInput != null)
            {
                if (_deviceOutput != null)
                {
                    _fileInput.RemoveOutgoingConnection(_deviceOutput);
                }

                _fileInput.FileCompleted -= FileInput_FileCompleted;
                _fileInput.Dispose();
            }
        }
    }
}

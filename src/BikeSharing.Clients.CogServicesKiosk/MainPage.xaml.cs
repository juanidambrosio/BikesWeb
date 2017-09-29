using BikeSharing.Clients.CogServicesKiosk.Data;
using BikeSharing.Clients.CogServicesKiosk.Models;
using Microsoft.Cognitive.LUIS;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.SpeakerRecognition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace BikeSharing.Clients.CogServicesKiosk
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        #region Variables

        public event PropertyChangedEventHandler PropertyChanged;

        private DisplayRequest _displayRequest;
        private MediaCapture _mediaCapture;
        private CancellationTokenSource _cts;
        private Task _videoMonitoringTask = null;

        #endregion

        #region Properties

        private UserProfile _User;
        public UserProfile User
        {
            get { return _User; }
            private set
            {
                if(this.SetProperty(ref _User, value))
                    this.IsVoiceVerified = false;
            }
        }

        private bool _IsVoiceVerified;
        public bool IsVoiceVerified
        {
            get { return _IsVoiceVerified; }
            private set { this.SetProperty(ref _IsVoiceVerified, value); }
        }

        private bool _ShowMicrophone;
        public bool ShowMicrophone
        {
            get { return _ShowMicrophone; }
            private set { this.SetProperty(ref _ShowMicrophone, value); }
        }

        private string _MicrophoneText;
        public string MicrophoneText
        {
            get { return _MicrophoneText; }
            private set { this.SetProperty(ref _MicrophoneText, value); }
        }

        #endregion

        #region Constructor

        public MainPage()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        #region NavigatedTo / NavigatedFrom

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            _cts = new CancellationTokenSource();

            // Landscape preference
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

            // Keeps the screen alive i.e. prevents screen from going to sleep
            _displayRequest = new DisplayRequest();
            _displayRequest.RequestActive();

            // Find all the video cameras on the device
            var cameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // Choose the front facing camera else choose the first available
            var preferredCamera = cameras.SingleOrDefault(deviceInfo => deviceInfo.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front) ?? cameras.FirstOrDefault();
            if (preferredCamera == null)
            {
                Debug.WriteLine("No camera found on device!");
                return;
            }

            // Initialize and start the camera video stream into the app preview window
            _mediaCapture = new MediaCapture();
            await _mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings()
            {
                StreamingCaptureMode = StreamingCaptureMode.Video,
                VideoDeviceId = preferredCamera.Id
            });
            videoPreview.Source = _mediaCapture;
            await _mediaCapture.StartPreviewAsync();

            // Initiate monitoring of the video stream for faces
            _videoMonitoringTask = this.MonitorVideoStreamAsync(_cts.Token);

            base.OnNavigatedTo(e);
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Cancel all tasks that are running
            _cts.Cancel();

            // Wait for the main video monitoring task to complete
            await _videoMonitoringTask;

            // Allows the screen to go to sleep again when you leave this page
            _displayRequest.RequestRelease();
            _displayRequest = null;

            // Stop and clean up the video feed
            await _mediaCapture.StopPreviewAsync();
            videoPreview.Source = null;
            _mediaCapture.Dispose();
            _mediaCapture = null;

            base.OnNavigatedFrom(e);
        }

        #endregion

        #region Video Stream Monitoring

        private async Task MonitorVideoStreamAsync(CancellationToken ct)
        {
            // Continue looping / watching the video stream until this page asks to stop via the cancellation token
            while (ct.IsCancellationRequested == false)
            {
                try
                {
                    // Capture a frame from the video feed
                    var mediaProperties = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
                    var videoFrame = new VideoFrame(BitmapPixelFormat.Rgba16, (int)mediaProperties.Width, (int)mediaProperties.Height);
                    await _mediaCapture.GetPreviewFrameAsync(videoFrame);

                    //await this.FaceDetectionWithFeatureAnalysisAsync(videoFrame.SoftwareBitmap);
                    //continue;
                    
                    // Detect faces in frame bitmap
                    var faces = await this.FaceDetectionAsync(videoFrame.SoftwareBitmap);

                    // Only do face verification if there is a face to verify
                    if (faces?.Any() == true)
                    {
                        // Check to see if a user profile has already been identified
                        if (this.User == null)
                        {
                            // Draw face boxes on the UI
                            this.DrawFacesOnUI(videoFrame.SoftwareBitmap.PixelWidth, videoFrame.SoftwareBitmap.PixelHeight, faces);

                            // Identity faces in frame
                            this.User = await this.FaceVerificationAsync(faces.Select(s => s.FaceId)?.ToArray());

                            if (this.User?.VoiceProfileId.HasValue == true)
                                this.IsVoiceVerified = await this.SpeakerVerificationAsync(this.User.VoiceProfileId.Value, this.User.VoiceSecretPhrase);
                        }
                        else
                        {
                            // User is known....

                            // Emotions
                            var emotions = await this.EmotionDetectionAsync(videoFrame.SoftwareBitmap);
                            this.DrawFacesOnUI(videoFrame.SoftwareBitmap.PixelWidth, videoFrame.SoftwareBitmap.PixelHeight, emotions);

                            //var isSmiling = await this.IsCustomerSmilingAsync(videoFrame.SoftwareBitmap);
                            //Debug.WriteLine("IsSmiling: " + isSmiling);
                        }
                    }
                    else
                    {
                        facesCanvas.Children.Clear();
                        this.User = null;
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Error analyzing video frame: " + ex.ToString());
                }
            }
        }

        #endregion

        #region Cognitive Services - Face Detection

        private async Task<Microsoft.ProjectOxford.Face.Contract.Face[]> FaceDetectionAsync(SoftwareBitmap bitmap)
        {
            // Convert video frame image to a stream
            var stream = await bitmap.AsStream();

            // Cognitive Services Face API client from the Nuget package
            var client = new FaceServiceClient(App.FACE_API_SUBSCRIPTION_KEY);

            // Call Cognitive Services Face API to look for identity candidates in the bitmap image
            return await client.DetectAsync(stream);
        }

        private async Task FaceDetectionWithFeatureAnalysisAsync(SoftwareBitmap bitmap)
        {
            // Convert video frame image to a stream
            var stream = await bitmap.AsStream();

            // Cognitive Services Face API client from the Nuget package
            var client = new FaceServiceClient(App.FACE_API_SUBSCRIPTION_KEY);

            // Array of features that should be analyzed from the video still
            var features = new FaceAttributeType[]
                {
                    FaceAttributeType.Smile,
                    FaceAttributeType.Age,
                    FaceAttributeType.Gender,
                    FaceAttributeType.Glasses
                };

            // Ask Cognitive Services to analyze the picture and determine face attributes as specified in array
            var faces = await client.DetectAsync(
                imageStream: stream,
                returnFaceId: true,
                returnFaceLandmarks: false,
                returnFaceAttributes: features
                );

            // Remove previous faces on UI canvas
            this.ClearFacesOnUI();

            // Video feed is probably a different resolution than the actual window size, so scale the sizes of each face
            double widthScale = bitmap.PixelWidth / facesCanvas.ActualWidth;
            double heightScale = bitmap.PixelHeight / facesCanvas.ActualHeight;

            // Draw a box for each face detected w/ text of face features
            foreach(var face in faces)
                this.DrawFaceOnUI(widthScale, heightScale, face);
        }

        private void DrawFaceOnUI(double widthScale, double heightScale, Microsoft.ProjectOxford.Face.Contract.Face face)
        {
            Rectangle box = new Rectangle();
            box.Width = (uint)(face.FaceRectangle.Width / widthScale);
            box.Height = (uint)(face.FaceRectangle.Height / heightScale);
            box.Fill = new SolidColorBrush(Colors.Transparent);
            box.Stroke = new SolidColorBrush(Colors.Lime);
            box.StrokeThickness = 2;
            box.Margin = new Thickness((uint)(face.FaceRectangle.Left / widthScale), (uint)(face.FaceRectangle.Top / heightScale), 0, 0);
            facesCanvas.Children.Add(box);

            // Add face attributes found
            var tb = new TextBlock();
            tb.Foreground = new SolidColorBrush(Colors.Lime);
            tb.Padding = new Thickness(4);
            tb.Margin = new Thickness((uint)(face.FaceRectangle.Left / widthScale), (uint)(face.FaceRectangle.Top / heightScale), 0, 0);

            if (face.FaceAttributes.Age > 0)
                tb.Text += "Age: " + face.FaceAttributes.Age + Environment.NewLine;

            if (!string.IsNullOrEmpty(face.FaceAttributes.Gender))
                tb.Text += "Gender: " + face.FaceAttributes.Gender + Environment.NewLine;

            if (face.FaceAttributes.Smile > 0)
                tb.Text += "Smile: " + face.FaceAttributes.Smile + Environment.NewLine;

            tb.Text += "Glasses: " + face.FaceAttributes.Glasses + Environment.NewLine;

            if (face.FaceAttributes.FacialHair != null)
            {
                tb.Text += "Beard: " + face.FaceAttributes.FacialHair.Beard + Environment.NewLine;
                tb.Text += "Moustache: " + face.FaceAttributes.FacialHair.Moustache + Environment.NewLine;
                tb.Text += "Sideburns: " + face.FaceAttributes.FacialHair.Sideburns + Environment.NewLine;
            }

            facesCanvas.Children.Add(tb);
        }

        private void DrawFacesOnUI(int frameWidth, int frameHeight, Microsoft.ProjectOxford.Face.Contract.Face[] faces)
        {
            this.ClearFacesOnUI();

            if (faces == null)
                return;

            // Video feed is probably a different resolution than the actual window size, so scale the sizes of each face
            double widthScale = frameWidth / facesCanvas.ActualWidth;
            double heightScale = frameHeight / facesCanvas.ActualHeight;

            // Draw each face
            foreach (var face in faces)
                this.DrawFaceOnUI(widthScale, heightScale, face);
        }

        private void ClearFacesOnUI()
        {
            facesCanvas.Children.Clear();
        }

        private async Task<bool> IsCustomerSmilingAsync(SoftwareBitmap bitmap)
        {
            // Convert video frame image to a stream
            var stream = await bitmap.AsStream();

            // Call Cognitive Services Face API to look for identity candidates in the bitmap image
            var client = new FaceServiceClient(App.FACE_API_SUBSCRIPTION_KEY);

            // Ask Cognitive Services to also analyze the picture for smiles on the face
            var faces = await client.DetectAsync(
                imageStream: stream,
                returnFaceId: true,
                returnFaceLandmarks: false,
                returnFaceAttributes: new FaceAttributeType[] { FaceAttributeType.Smile }
                );

            // If a face was found, check to see if the confidence of the smile is at least 75%
            if (faces?.Any() == true)
                return faces[0].FaceAttributes.Smile > .75;
            else
                return false;
        }

        #endregion

        #region Cognitive Services - Face Verification

        private async Task<UserProfile> FaceVerificationAsync(Guid[] faceIDs)
        {
            if (faceIDs == null || faceIDs.Length == 0)
                return null;

            // Call Cognitive Services Face API to look for identity candidates in the bitmap image
            FaceServiceClient client = new FaceServiceClient(App.FACE_API_SUBSCRIPTION_KEY);
            var identityResults = await client.IdentifyAsync(App.FACE_API_GROUPID, faceIDs, confidenceThreshold: 0.6f);
            
            // Get the candidate with the highest confidence or null
            var candidate = identityResults.FirstOrDefault()?.Candidates?.OrderByDescending(o => o.Confidence).FirstOrDefault();

            // If candidate found, take the face ID and lookup in our customer database
            if (candidate != null)
                return await UserLookupService.Instance.GetUserByFaceID(candidate.PersonId);
            else
                return null;
        }

        #endregion

        #region Cognitive Services - Emotion Detection

        private async Task<Microsoft.ProjectOxford.Emotion.Contract.Emotion[]> EmotionDetectionAsync(SoftwareBitmap bitmap)
        {
            // Convert video frame image to a stream
            var stream = await bitmap.AsStream();

            // Use the Emotion API nuget package to access to the Cognitive Services Emotions service
            var client = new EmotionServiceClient(App.EMOTION_API_SUBSCRIPTION_KEY);

            // Pass the video frame image as a stream to the Emotion API to find all face/emotions in the video still
            return await client.RecognizeAsync(stream);
        }

        private void DrawFacesOnUI(int frameWidth, int frameHeight, Microsoft.ProjectOxford.Emotion.Contract.Emotion[] emotions)
        {
            facesCanvas.Children.Clear();

            if (emotions == null)
                return;

            // Video feed is probably a different resolution than the actual window size, so scale the sizes of each face
            double widthScale = frameWidth / facesCanvas.ActualWidth;
            double heightScale = frameHeight / facesCanvas.ActualHeight;

            // Draw each face
            foreach (var emotion in emotions)
            {
                // Draw the face box
                var box = new Rectangle();
                box.Width = (uint)(emotion.FaceRectangle.Width / widthScale);
                box.Height = (uint)(emotion.FaceRectangle.Height / heightScale);
                box.Fill = new SolidColorBrush(Colors.Transparent);
                box.Stroke = new SolidColorBrush(Colors.Red);
                box.StrokeThickness = 2;
                box.Margin = new Thickness((uint)(emotion.FaceRectangle.Left / widthScale), (uint)(emotion.FaceRectangle.Top / heightScale), 0, 0);
                facesCanvas.Children.Add(box);

                // Write the list of emotions in the facebook
                var tb = new TextBlock();
                tb.Foreground = new SolidColorBrush(Colors.Yellow);
                tb.Padding = new Thickness(4);
                tb.Margin = new Thickness((uint)(emotion.FaceRectangle.Left / widthScale) + box.Width, (uint)(emotion.FaceRectangle.Top / heightScale), 0, 0);
                
                tb.Text += "Anger: " + emotion.Scores.Anger + Environment.NewLine;
                tb.Text += "Contempt: " + emotion.Scores.Contempt + Environment.NewLine;
                tb.Text += "Disgust: " + emotion.Scores.Disgust + Environment.NewLine;
                tb.Text += "Fear: " + emotion.Scores.Fear + Environment.NewLine;
                tb.Text += "Happiness: " + emotion.Scores.Happiness + Environment.NewLine;
                tb.Text += "Neutral: " + emotion.Scores.Neutral + Environment.NewLine;
                tb.Text += "Sadness: " + emotion.Scores.Sadness + Environment.NewLine;
                tb.Text += "Surprise: " + emotion.Scores.Surprise + Environment.NewLine;
                
                facesCanvas.Children.Add(tb);
            }
        }

        #endregion

        #region Cognitive Services - Speaker Verification

        private async Task<bool> SpeakerVerificationAsync(Guid speakerProfileID, string verificationPhrase)
        {
            // Prompt the user to record an audio stream of their phrase
            var audioStream = await this.PromptUserForVoicePhraseAsync(verificationPhrase);

            try
            {
                // Use the Speaker Verification API nuget package to access to the Cognitive Services Speaker Verification service
                var client = new SpeakerVerificationServiceClient(App.SPEAKER_RECOGNITION_API_SUBSCRIPTION_KEY);

                // Pass the audio stream and the user's profile ID to the service to have analyzed for match
                var response = await client.VerifyAsync(audioStream, speakerProfileID);

                // Check to see if the stream was accepted and then the confidence level Cognitive Services has that the speaker is a match to the profile specified
                if (response.Result == Microsoft.ProjectOxford.SpeakerRecognition.Contract.Verification.Result.Accept)
                    return response.Confidence >= Microsoft.ProjectOxford.SpeakerRecognition.Contract.Confidence.Normal;
                else
                    return false;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error during SpeakerVerificationAsync: " + ex.ToString());
                return false;
            }
        }

        private async Task<Stream> PromptUserForVoicePhraseAsync(string verificationPhrase)
        {
            try
            {
                this.ShowMicrophone = true;
                this.MicrophoneText = verificationPhrase;

                // Wrapper object to get sound from the microphone
                var recorder = new AudioRecorder();

                // Records sound from the microphone for the specified amount of time
                return await recorder.RecordAsync(TimeSpan.FromSeconds(5));
            }
            finally
            {
                this.MicrophoneText = null;
                this.ShowMicrophone = false;
            }
        }

        #endregion

        #region Cognitive Services - LUIS (Language Understanding Intelligence Service)

        private async Task<bool> ProcessCustomerTextAsync(string userSpokenText)
        {
            // Use the LUIS API nuget package to access to the Cognitive Services LUIS service
            var client = new LuisClient(App.LUIS_APP_ID, App.LUIS_SUBCRIPTION_KEY);

            // Pass the phrase spoken by the user to the service to determine the user's intent
            var result = await client.Predict(userSpokenText);

            // Run the appropriate business logic in response to the user's language intent
            switch (result?.TopScoringIntent?.Name)
            {
                case "rentBike":
                    await this.PerformRentBikeAsync();
                    return true;

                case "returnBike":
                    await this.PerformReturnBikeAsync();
                    return true;

                case "extendRental":
                    await this.PerformExtendRentalAsync();
                    return true;

                case "contactCustomerService":
                    await this.PerformContactCustomerServiceAsync();
                    return true;

                default:
                    // Text wasn't recognized, run default logic
                    await this.UnrecognizedIntentAsync();
                    return false;
            }
        }

        #endregion

        #region Business Logic

        private Task PerformRentBikeAsync()
        {
            return Task.CompletedTask;
        }

        private Task PerformReturnBikeAsync()
        {
            return Task.CompletedTask;
        }

        private Task PerformExtendRentalAsync()
        {
            return Task.CompletedTask;
        }

        private Task PerformContactCustomerServiceAsync()
        {
            return Task.CompletedTask;
        }

        private Task UnrecognizedIntentAsync()
        {
            return Task.CompletedTask;
        }

        #endregion

        #region Data Binding

        /// <summary>
        /// Runs a function on the currently executing platform's UI thread.
        /// </summary>
        /// <param name="action">Code to be executed on the UI thread</param>
        /// <param name="priority">Priority to indicate to the system when to prioritize the execution of the code</param>
        /// <returns>Task representing the code to be executing</returns>
        private void InvokeOnUIThread(System.Action action, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            var _ = this.InvokeOnUIThreadAsync(action, priority);
        }

        private async Task InvokeOnUIThreadAsync(System.Action action, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            if (this.Dispatcher == null || this.Dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                // Execute asynchronously on the thread the Dispatcher is associated with.
                await this.Dispatcher.RunAsync(priority, () => action());
            }
        }

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            else
            {
                storage = value;
                this.NotifyPropertyChanged(propertyName);
                return true;
            }
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #endregion
    }
}
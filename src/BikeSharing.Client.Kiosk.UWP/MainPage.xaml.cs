using BikeSharing.Client.Kiosk.UWP.AudioVideoProcessors;
using BikeSharing.Client.Kiosk.UWP.BikeSharingServices;
using BikeSharing.Client.Kiosk.UWP.STT;
using BikeSharing.Client.Kiosk.UWP.TTS;
using Microsoft.Cognitive.LUIS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.FaceAnalysis;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Input;
using Windows.Media.SpeechSynthesis;

namespace BikeSharing.Client.Kiosk.UWP
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        #region Variables

        public event PropertyChangedEventHandler PropertyChanged;

        private int KIOSK_ID = 72;
        private string PROFILES_SERVICE_BASE_URI = "http://bikesharing360-profiles-dev.azurewebsites.net";
        private string RIDES_SERVICE_BASE_URI = "http://bikesharing360-rides-dev.azurewebsites.net";

        private int MAX_SPEAKER_VERIFICATION_ATTEMPTS = 3;
        private float _faceRelativeSizeThreshold = 0.08f;
        private float _emotionScoreThreshold = 0.8f;
        private int RECORDING_TIME_FOR_VOICE_VERIFICATION_PHRASE = 5;
        private string _SpeechToTextLanguageName = "en-US";
        private int _SpeechToTextInitialSilenceTimeoutInSeconds = 6;
        private int _SpeechToTextBabbleTimeoutInSeconds = 0;
        private int _SpeechToTextEndSilenceTimeoutInSeconds = 3;
        private float LUIS_INTENT_SCORE_THRESHOLD = 0.6f;
        private bool _showingSpeechToTextUI = false;
        private bool _showSpeechPartialResult = true;
        private int NUMBER_FRAMES_TO_WAIT_BEFORE_STANDBY = 50;

        private MediaCapture _mediaCapture;
        private DisplayRequest _displayRequest;

        private FaceIdentificationProcessor _faceIdentificationProcessor;
        private SpeakerVerificationProcessor _speakerVerificationProcessor;
        private EmotionDetectionProcessor _emotionDetectionProcessor;
        private TextToSpeech _textToSpeech;
        private SpeechToText _speechToText;
        
        private TaskCompletionSource<bool> _uiInteractionCompletionSource;
        private TaskCompletionSource<bool> _playAudioTaskCompletionSource;
        //TODO (yumeng): remove _tokenSource usage. Demo only for restarting the session.
        private CancellationTokenSource _tokenSource;

        #endregion

        #region Properties

        private bool _IsBusy;
        public bool IsBusy
        {
            get { return _IsBusy; }
            private set { this.SetProperty(ref _IsBusy, value); }
        }

        private bool _ShowMicrophone;
        public bool ShowMicrophone
        {
            get { return _ShowMicrophone; }
            private set { this.SetProperty(ref _ShowMicrophone, value); }
        }

        private string _KioskMessage;
        public string KioskMessage
        {
            get { return _KioskMessage; }
            private set { this.SetProperty(ref _KioskMessage, value); }
        }

        private string _CustomerMessage;
        public string CustomerMessage
        {
            get { return _CustomerMessage; }
            private set { this.SetProperty(ref _CustomerMessage, value); }
        }

        private bool _ShowVoiceVerificationPassedIcon;
        public bool ShowVoiceVerificationPassedIcon
        {
            get { return _ShowVoiceVerificationPassedIcon; }
            private set { this.SetProperty(ref _ShowVoiceVerificationPassedIcon, value); }
        }

        private string _VoiceVerificationPhrase;
        public string VoiceVerificationPhrase
        {
            get { return _VoiceVerificationPhrase; }
            private set { this.SetProperty(ref _VoiceVerificationPhrase, value); }
        }

        private bool _ShowRetryButton;
        public bool ShowRetryButton
        {
            get { return _ShowRetryButton; }
            private set { this.SetProperty(ref _ShowRetryButton, value); }
        }

        private bool _ShowResetButton;
        public bool ShowResetButton
        {
            get { return _ShowResetButton; }
            private set { this.SetProperty(ref _ShowResetButton, value); }
        }

        #endregion

        #region Constructors

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            this.Unloaded += MainPage_Unloaded;
        }

        #endregion

        #region Methods

        #region  Startup/Cleanup

        private async Task InitializeAsync()
        {
            try
            {
                this.IsBusy = true;

                // Load configuration values
                await this.LoadConfigurationAsync();

                // Find all the video cameras on the device
                var cameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

                // Choose the front facing camera else choose the first available
                var preferredCamera = cameras.SingleOrDefault(deviceInfo => deviceInfo.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front) ?? cameras.FirstOrDefault();

                if (preferredCamera == null)
                {
                    Debug.WriteLine("No camera found on device!");
                    return;
                }

                // Initiate the camera feed into the preview window
                _mediaCapture = new MediaCapture();
                await _mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings()
                {
                    StreamingCaptureMode = StreamingCaptureMode.Video,
                    VideoDeviceId = preferredCamera.Id
                });
                _mediaCapture.Failed += MediaCapture_Failed;
                videoPreview.Source = _mediaCapture;
                await _mediaCapture.StartPreviewAsync();

                // Keeps the screen awake
                _displayRequest = new DisplayRequest();
                _displayRequest.RequestActive();

                // Landscape preference
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

                if (_faceIdentificationProcessor == null)
                {
                    try
                    {
                        _faceIdentificationProcessor = new FaceIdentificationProcessor(_faceRelativeSizeThreshold);
                        await _faceIdentificationProcessor.InitializeAsync();
                    }
                    catch (Exception) { }
                }

                if (_speakerVerificationProcessor == null)
                {
                    try
                    {
                        _speakerVerificationProcessor = new SpeakerVerificationProcessor();
                        await _speakerVerificationProcessor.InitializeAsync();
                    }
                    catch (Exception) { }
                }

                if (_emotionDetectionProcessor == null)
                {
                    try
                    {
                        _emotionDetectionProcessor = new EmotionDetectionProcessor();
                    }
                    catch (Exception) { }
                }

                if (_textToSpeech == null)
                {
                    try
                    {
                        _textToSpeech = new TTS.TextToSpeech();
                        await _textToSpeech.InitializeSynthesizerAsync();
                    }
                    catch (Exception) { }
                }

                if (_speechToText == null)
                {
                    try
                    {
                        _speechToText = new SpeechToText(
                            _SpeechToTextLanguageName,
                            _SpeechToTextInitialSilenceTimeoutInSeconds,
                            _SpeechToTextBabbleTimeoutInSeconds,
                            _SpeechToTextEndSilenceTimeoutInSeconds);
                        await _speechToText.InitializeRecognizerAsync();
                        _speechToText.OnHypothesis += _speechToText_OnHypothesis;
                        _speechToText.CapturingStarted += _speechToText_CapturingStarted;
                        _speechToText.CapturingEnded += _speechToText_CapturingEnded;
                    }
                    catch (Exception) { }
                }

                if (_uwpSynthesizer == null)
                {
                    try
                    {
                        // Get all of the installed voices.
                        var voices = SpeechSynthesizer.AllVoices;
                        var ziraVoice = voices.FirstOrDefault(v => v.DisplayName.Contains("Zira"));
                        if (ziraVoice == null) ziraVoice = voices.First();
                        _uwpSynthesizer = new SpeechSynthesizer();
                        _uwpSynthesizer.Voice = ziraVoice;
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Failed to InitializeAsync. {0}", ex));
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private async Task MainLoopAsync()
        {
            this.GoToVisualState("Standby");
            await this.InitializeAsync();

            if(_mediaCapture == null)
            {
                Debug.WriteLine("Cannot run kiosk on this device as there is no video camera stream available.");
                return;
            }

            this.IsBusy = true;
            var previewProperties = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

            while (true)
            {
                this.IsBusy = false;

                try
                {
                    _tokenSource = new CancellationTokenSource();
                    txtWelcomeName.Text = "Welcome to BikeSharing360!";

                    // Face recognition loop
                    var face = await this.FindFaceAsync(previewProperties, _tokenSource.Token);

                    //Hard-code Lara
                    var recognizedFaceProfileId = "4f655429-63c0-4880-957e-8ef01c47bc6b";

                    if (face != null)
                    {
                        var candidates = face.IdentifyResults.FirstOrDefault().Candidates;
                        if (candidates == null || candidates.Any() == false)
                        {
                            // New, unknown customer is present
                            txtWelcomeName.Text = "Hello new friend!";
                            await this.DisplayKioskMessageAsync(Strings.Kiosk.new_customer);
                            this.GoToVisualState("Standby");
                            continue;
                        }

                        // Retrieve customer's profile based on pre-registered face profile.
                        recognizedFaceProfileId = candidates.OrderByDescending(c => c.Confidence)
                            .First()
                            .PersonId
                            .ToString()
                            .ToLowerInvariant();
                    }

                    // Lookup the profile in the customer profiles API
                    var profileService = new UserProfileServices(PROFILES_SERVICE_BASE_URI);
                    var profile = await profileService.GetUserProfileByFaceProfileIdAsync(recognizedFaceProfileId);

                    if (profile == null)
                    {
                        // Fail to retrieve usre profile from the backend profile services, for example, due to network issue
                        await this.DisplayKioskMessageAsync(Strings.Kiosk.fail_to_retrieve_profile);
                        this.GoToVisualState("Standby");
                        continue;
                    }
                    else
                    {
                        // Known customer is present
                        txtWelcomeName.Text = string.Format("Hello, {0}!", profile.FirstName);
                        await this.DisplayKioskMessageAsync(string.Format(Strings.Kiosk.greeting, profile.FirstName));

                        if (profile.VoiceProfileId.HasValue)
                        {
                            // Calling Speaker Recognition service to do the verification
                            var speakerVerificationPassed = await this.VerifySpeakerAsync(profile, _tokenSource.Token);
                            if (speakerVerificationPassed == false)
                            {
                                await this.DisplayKioskMessageAsync(Strings.Kiosk.voice_verification_failed);
                                await this.ResetSessionAsync();
                                continue;
                            }
                        }

                        await this.DisplayKioskMessageAsync(string.Format(Strings.Kiosk.what_to_do, profile.FirstName));

                        // Convert customer speech into text, call Luis service to analyze the intent, then process it
                        bool intentProcessed = false;
                        do
                        {
                            _tokenSource.Token.ThrowIfCancellationRequested();
                            // Clear the customer's previous spoken words if any from the UI
                            this.CustomerMessage = null;
                            string userSpokenText = null;
                                // Get text from customer's speech
                            userSpokenText = await _speechToText.GetTextFromSpeechAsync(_showingSpeechToTextUI);

                            this.CustomerMessage = userSpokenText;

                            await this.DisplayKioskMessageAsync("Thinking...", false);

                            // Predict the customer's intent using LUIS service
                            intentProcessed = await this.ProcessCustomerTextAsync(userSpokenText, profile);
                        }
                        while (!intentProcessed);

                        // Clear the customer's spoken words from the UI
                        this.CustomerMessage = null;

                        // Call Emotion Api to detect customer's emotion until he/she smiles
                        await this.DisplayKioskMessageAsync(Strings.Kiosk.emotion_prompt);
                        await this.DetectEmotionLoopAsync(previewProperties, _tokenSource.Token);

                        await this.ResetSessionAsync(10);
                    }
                }
                catch (OperationCanceledException)
                {
                    await this.ResetSessionAsync(delayToReset: 0, delayAfterStandby: 5, withGoodByeMessage: false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("Failure while looping in MainLoopAsync. {0}", ex));
                }
                finally
                {
                    _tokenSource.Dispose();
                    _tokenSource = null;
                }

                this.IsBusy = false;
            }
        }

        private async Task CleanupAsync()
        {
            try
            {
                this.IsBusy = true;

                if (_mediaCapture != null)
                {
                    await _mediaCapture.StopPreviewAsync();

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        videoPreview.Source = null;
                        _displayRequest?.RequestRelease();
                    });

                    _mediaCapture.Dispose();
                    _mediaCapture = null;
                }

                if (_textToSpeech != null)
                {
                    _textToSpeech.Dispose();
                    _textToSpeech = null;
                }

                if (_speechToText != null)
                {
                    _speechToText.OnHypothesis -= _speechToText_OnHypothesis;
                    _speechToText.CapturingStarted -= _speechToText_CapturingStarted;
                    _speechToText.CapturingEnded -= _speechToText_CapturingEnded;
                    _speechToText.Dispose();
                    _speechToText = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Failed to CleanupAsync. {0}", ex));
            }
            finally
            {
                this.GoToVisualState("Standby");
                this.IsBusy = false;
            }
        }

        private async Task ResetSessionAsync(int delayToReset = 3, int delayAfterStandby = 0, bool withGoodByeMessage = true)
        {
            if (withGoodByeMessage)
                await this.DisplayKioskMessageAsync("Goodbye!");

            await Task.Delay(TimeSpan.FromSeconds(delayToReset));
            this.GoToVisualState("Standby");
            this.KioskMessage = null;
            this.CustomerMessage = null;
            if (_uiInteractionCompletionSource != null && !_uiInteractionCompletionSource.Task.IsCompleted)
                _uiInteractionCompletionSource.SetResult(false);
            await Task.Delay(TimeSpan.FromSeconds(delayAfterStandby));
            this.IsBusy = false;
        }

        #endregion

        #region Process Customer Text

        /// <summary>
        /// Process a customer's spoken text
        /// </summary>
        /// <param name="userSpokenText">Text spoken</param>
        /// <param name="profile">Customer's profile</param>
        /// <returns>true if the customer's request can be processed; false otherwise.</returns>
        private async Task<bool> ProcessCustomerTextAsync(string userSpokenText, UserProfile profile)
        {
            var luisClient = new LuisClient(App.COGNITIVE_SERVICES_LUIS_APP_ID, App.COGNITIVE_SERVICES_LUIS_SUBCRIPTION_KEY);
            Debug.WriteLine($"Predicting intent on '{userSpokenText}'...");

            string intent = null;

            try
            {
                var result = await luisClient.Predict(userSpokenText);

                if (result == null || result.TopScoringIntent == null)
                {
                    intent = string.Empty;
                }
                else
                {
                    Debug.WriteLine($"Top scoring intent: {result.TopScoringIntent.Name} - {result.TopScoringIntent.Score}");

                    if (result.TopScoringIntent.Score >= LUIS_INTENT_SCORE_THRESHOLD)
                        intent = result.TopScoringIntent.Name;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occurred: {ex.ToString()}");
                intent = "rentBike";
            }

            switch (intent)
            {
                case "rentBike":
                    await this.PerformRentBikeAsync(profile);
                    break;

                case "returnBike":
                    await this.PerformReturnBikeAsync(profile);
                    break;

                case "extendRental":
                    await this.PerformExtendRentalAsync(profile);
                    break;

                case "contactCustomerService":
                    await this.PerformContactCustomerServiceAsync(profile);
                    break;

                default:
                    await this.UnrecognizedIntentAsync();
                    return false;
            }

            return true;
        }

        private async Task PerformRentBikeAsync(UserProfile profile)
        {
            Debug.WriteLine($"Customer {profile.FirstName} is renting a bike.");

            var confirmation = await this.CheckoutBikeAsync(KIOSK_ID, profile);
            var text = string.Empty;
            if (confirmation?.ResultType == ResultType.Succeeded)
            {
                var bikes = string.Join(",", confirmation.Bikes);
                text = string.Format(Strings.Kiosk.bike_unlocked, profile.FirstName);
            }
            else
            {
                text = string.Format(Strings.Kiosk.checkout_failed, profile.FirstName);
            }
            await this.DisplayKioskMessageAsync(text);
        }

        private async Task<CheckoutConfirmation> CheckoutBikeAsync(int bikeStationId, UserProfile profile)
        {
            RideServices stationServiceClient = new RideServices(RIDES_SERVICE_BASE_URI);
            // For this demo we are only checking out one bike for the customer.
            var confirmation = await stationServiceClient.CheckoutBikes(bikeStationId, profile.UserId, 1);

            return confirmation;
        }

        private async Task PerformReturnBikeAsync(UserProfile profile)
        {
            // Not implemented in this demo
            Debug.WriteLine($"Customer {profile.FirstName} is returning a bike.");
            await this.DisplayKioskMessageAsync("Thank you for returning the bike! Please place it in an open slot in the rack right and your rental will be completed.");
        }

        private async Task PerformExtendRentalAsync(UserProfile profile)
        {
            // Not implemented in this demo
            Debug.WriteLine($"Customer {profile.FirstName} is extending current rental.");
            await this.DisplayKioskMessageAsync("Your current rental has been extended.");
        }

        private async Task PerformContactCustomerServiceAsync(UserProfile profile)
        {
            // Not implemented in this demo
            Debug.WriteLine($"Customer {profile.FirstName} is Contacting Customer Services.");
            await this.DisplayKioskMessageAsync("Customer service will call you momentarily...");
        }

        private async Task UnrecognizedIntentAsync()
        {
            this.CustomerMessage = null;
            await this.DisplayKioskMessageAsync(Strings.Kiosk.unrecognized_intent);
        }

        #endregion

        private async Task<bool> VerifySpeakerAsync(UserProfile profile, CancellationToken token)
        {
            int i = MAX_SPEAKER_VERIFICATION_ATTEMPTS;
            while (i > 0)
            {
                token.ThrowIfCancellationRequested();
                bool isVerified = false;
                try
                {
                    // Provide user instructions
                    string prompt = (i == MAX_SPEAKER_VERIFICATION_ATTEMPTS) ? Strings.Kiosk.voice_verification_prompt : string.Format(Strings.Kiosk.voice_verification_prompt_2, RECORDING_TIME_FOR_VOICE_VERIFICATION_PHRASE);
                    await this.DisplayKioskMessageAsync(prompt);

                    // Show verification phrase
                    this.VoiceVerificationPhrase = profile.VoiceSecretPhrase;
                    this.ShowMicrophone = true;

                    // Begin recording of voice
                    isVerified = await _speakerVerificationProcessor.VerifyAsync(profile.VoiceProfileId.Value, TimeSpan.FromSeconds(RECORDING_TIME_FOR_VOICE_VERIFICATION_PHRASE));
                    //TODO (yumeng): demo-only, remove before publishing
                    isVerified = true;
                }
                finally
                {
                    this.VoiceVerificationPhrase = null;
                    this.ShowMicrophone = false;
                }

                if (isVerified)
                {
                    this.ShowVoiceVerificationPassedIcon = true;
                    await this.DisplayKioskMessageAsync(Strings.Kiosk.voice_verification_attempt_passed);
                    this.ShowVoiceVerificationPassedIcon = false;
                    return true;
                }
                else
                {
                    i--;
                    try
                    {
                        if (i > 0)
                        {
                            await this.DisplayKioskMessageAsync(Strings.Kiosk.voice_verification_attempt_failed);
                            this.ShowRetryButton = true;
                            _uiInteractionCompletionSource = new TaskCompletionSource<bool>();
                            await _uiInteractionCompletionSource.Task;
                        }
                        else
                            break;
                    }
                    finally
                    {
                        _uiInteractionCompletionSource = null;
                        this.ShowRetryButton = false;
                    }
                }
            }

            return false;
        }

        private async Task DetectEmotionLoopAsync(VideoEncodingProperties properties, CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                // Don't capture video too often.
                await Task.Delay(TimeSpan.FromMilliseconds(200));

                var videoFrame = new VideoFrame(BitmapPixelFormat.Rgba16, (int)properties.Width, (int)properties.Height);
                try
                {
                    // Capture a frame from the video feed
                    await _mediaCapture.GetPreviewFrameAsync(videoFrame);
                    
                    var emotions = await _emotionDetectionProcessor.DetectEmotionAsync(videoFrame.SoftwareBitmap);
                    if (emotions != null)
                    {
                        // For this demo we assume that there is only one customer using the Kiosk at a time.
                        var firstEmotion = emotions.First();
                        
                        await this.DisplayFacesInPreviewBox(videoFrame, firstEmotion);

                        if (firstEmotion.EmotionType == EmotionType.Happiness && firstEmotion.Score > _emotionScoreThreshold)
                        {
                            await this.DisplayKioskMessageAsync(Strings.Kiosk.enjoy_ride);
                            return;
                        }
                    }
                }
                finally
                {
                    videoFrame.Dispose();
                }
            }
        }

        private async Task DisplayFacesInPreviewBox(VideoFrame videoFrame, EmotionDetectionResult emotion = null)
        {
            var detectedFacesResult = await _faceIdentificationProcessor.GetDetectedFacesAsync(videoFrame.SoftwareBitmap);
            this.ShowDetectedFaces(videoFrame.SoftwareBitmap, detectedFacesResult, emotion?.ToString());
        }
        
        private async Task DisplayKioskMessageAsync(string message = null, bool speakText = true)
        {
            this.KioskMessage = message;

            if (speakText && !string.IsNullOrWhiteSpace(message))
            {
                try
                {
                    await _textToSpeech.SpeakAsync(message);
                }
                catch
                {
                    await TextToSpeechFallBackAsync(message);
                }
            }
        }

        #region Video Preview Box

        private async Task<FaceIdentificationResult> FindFaceAsync(VideoEncodingProperties previewProperties, CancellationToken token)
        {
            Debug.WriteLine("IdentifyFacesLoopAsync started...");
            FaceIdentificationResult result = null;

            // to avoid frequent hiding-then-reshowing UI, we only go to Standby mode
            // after the kiosk hasn't detected faces for a certain number of frames in row
            int numberOfVideoFramesWithoutFaces = 0;
            int numberOfTries = 0;

            while (true)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(TimeSpan.FromMilliseconds(100));  // No need to capture too often.

                var videoFrame = new VideoFrame(BitmapPixelFormat.Rgba16, (int)previewProperties.Width, (int)previewProperties.Height);
                try
                {
                    await _mediaCapture.GetPreviewFrameAsync(videoFrame);

                    var detectResult = await _faceIdentificationProcessor.GetDetectedFacesAsync(videoFrame.SoftwareBitmap);

                    this.ShowDetectedFaces(videoFrame.SoftwareBitmap, detectResult);

                    switch (detectResult.ResultType)
                    {
                        case FaceResultType.FaceTooSmall:
                            this.GoToVisualState("CustomerPresent");
                            // Face(s) too small, which means the customer is still far away from the camera.
                            result = null;
                            Debug.WriteLine("Face too small.");
                            numberOfVideoFramesWithoutFaces = 0;
                            break;
                        case FaceResultType.Good:
                            this.GoToVisualState("CustomerPresent");

                            await this.DisplayKioskMessageAsync("Well hello there! Give me a second to identify you...", false);

                            numberOfTries++;
                            Debug.WriteLine($"Number of tries: {numberOfTries}");
                            result = await _faceIdentificationProcessor.IdentifyFaceAsync(videoFrame.SoftwareBitmap, detectResult.DetectedFaces);

                            // It's possible that the Cognitive Service Face API failed to identify any candicates in
                            // this video frame.
                            if (result == null || result.IdentifyResults.FirstOrDefault() == null
                                || !result.IdentifyResults.FirstOrDefault().Candidates.Any())
                            {
                                //TODO (yumeng): demo-only
                                if (numberOfTries > 1)
                                {
                                    Debug.WriteLine("Enough retries");
                                    result = null;
                                }
                                else
                                {
                                    Debug.WriteLine("Face not recognized.");
                                    continue;
                                }
                            }

                            Debug.WriteLine("Face found.");
                            numberOfVideoFramesWithoutFaces = 0;
                            Debug.WriteLine("IdentifyFacesLoopAsync completed!");
                            facesCanvas.Children.Clear();
                            return result;
                        case FaceResultType.NoFaceDetected:
                            numberOfVideoFramesWithoutFaces++;
                            Debug.WriteLine($"Number of video frames without faces: {numberOfVideoFramesWithoutFaces}");
                            if (numberOfVideoFramesWithoutFaces > NUMBER_FRAMES_TO_WAIT_BEFORE_STANDBY)
                            {
                                Debug.WriteLine("No faces detected.");
                                this.GoToVisualState("Standby");
                            }
                            break;
                    }
                }
                finally
                {
                    videoFrame.Dispose();
                }
            }

            Debug.WriteLine("IdentifyFacesLoopAsync aborted!");
            return null;
        }

        private async Task TextToSpeechFallBackAsync(string message)
        {
            Debug.WriteLine("Playing audio using UWP api...");
            // Create a stream from the text. This will be played using a media element.
            var synthesizedStream = await _uwpSynthesizer.SynthesizeTextToStreamAsync(message);
            // Set the source and start playing the synthesized audio stream.
            media.AutoPlay = true;
            media.SetSource(synthesizedStream, synthesizedStream.ContentType);
            _playAudioTaskCompletionSource = new TaskCompletionSource<bool>();
            media.Play();
            await _playAudioTaskCompletionSource.Task;
        }

        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            _playAudioTaskCompletionSource.SetResult(true);
            Debug.WriteLine("Done playing audio using UWP Api.");
        }

        private void ShowDetectedFaces(SoftwareBitmap sourceBitmap, FaceDetectionResult detectResult, string emotion = null)
        {
            facesCanvas.Children.Clear();

            var strokeColor = detectResult.ResultType == FaceResultType.FaceTooSmall && string.IsNullOrEmpty(emotion) ? Colors.Yellow : Colors.Green;
            var strokeBrush = new SolidColorBrush(strokeColor);

            if (detectResult.DetectedFaces != null)
            {
                double widthScale = sourceBitmap.PixelWidth / facesCanvas.ActualWidth;
                double heightScale = sourceBitmap.PixelHeight / facesCanvas.ActualHeight;

                foreach (DetectedFace face in detectResult.DetectedFaces)
                {
                    // Create a rectangle element for displaying the face box but since we're using a Canvas
                    // we must scale the rectangles according to the image’s actual size.
                    // The original FaceBox values are saved in the Rectangle's Tag field so we can update the
                    // boxes when the Canvas is resized.
                    Rectangle box = new Rectangle();
                    box.Tag = face.FaceBox;
                    box.Width = (uint)(face.FaceBox.Width / widthScale);
                    box.Height = (uint)(face.FaceBox.Height / heightScale);
                    box.Fill = new SolidColorBrush(Colors.Transparent);
                    box.Stroke = strokeBrush;
                    box.StrokeThickness = 2;
                    box.Margin = new Thickness((uint)(face.FaceBox.X / widthScale), (uint)(face.FaceBox.Y / heightScale), 0, 0);

                    facesCanvas.Children.Add(box);
                }

                if (emotion != null)
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = emotion;
                    textBlock.Foreground = new SolidColorBrush(Colors.White);
                    textBlock.Margin = new Thickness(1, 1, 0, 0);
                    textBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    textBlock.VerticalAlignment = VerticalAlignment.Top;

                    facesCanvas.Children.Add(textBlock);
                }
            }
        }

        #endregion

        #region Visual State

        private void GoToVisualState(string visualStateName)
        {
            if (this.Dispatcher.HasThreadAccess)
            {
                VisualStateManager.GoToState(this, visualStateName, false);
            }
            else
            {
                var _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    VisualStateManager.GoToState(this, visualStateName, false);
                });
            }
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

        #region Events

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Enable handling of key events.
            this.Focus(FocusState.Programmatic);
            try
            {
                await this.MainLoopAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MainPage_Loaded failed. {0}", ex);
                await this.CleanupAsync();
            }
        }

        private async void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            await this.CleanupAsync();
        }

        private void _speechToText_OnHypothesis(object sender, string e)
        {
            if (_showSpeechPartialResult)
            {
                this.InvokeOnUIThread(() =>
                {
                    this.CustomerMessage = e;
                });
            }
        }

        private void _speechToText_CapturingStarted(object sender, EventArgs e)
        {
            this.InvokeOnUIThread(() =>
            {
                this.ShowMicrophone = true;
            });
        }

        private void _speechToText_CapturingEnded(object sender, EventArgs e)
        {
            this.InvokeOnUIThread(() =>
            {
                this.ShowMicrophone = false;
            });
        }

        private void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            Debug.WriteLine("MediaCapture failed. {0}", errorEventArgs.Message);
        }

        private void btnRetry_Click(object sender, RoutedEventArgs e)
        {
            _uiInteractionCompletionSource?.SetResult(true);
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            _uiInteractionCompletionSource?.SetResult(true);
        }

        #endregion

        #region Temporary Demo Code

        //TODO (yumeng): demo only
        /// <summary>
        /// Allows overriding various kiosk settings via a local text file under Music folder.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> LoadConfigurationAsync()
        {
            // cannot use DocumentLibrary folder without user explicitly doing that
            var documentFolder = KnownFolders.MusicLibrary;

            StorageFile settingsFile;
            try
            {
                settingsFile = await documentFolder.GetFileAsync("kiosk_settings.txt");
            }
            catch
            {
                return false;
            }

            var lines = await FileIO.ReadLinesAsync(settingsFile);
            foreach (var line in lines)
            {
                var parts = line.Split(new[] { ' ', '\t', ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    try
                    {
                        if (parts[0] == nameof(PROFILES_SERVICE_BASE_URI)) PROFILES_SERVICE_BASE_URI = parts[1];
                        else if (parts[0] == nameof(RIDES_SERVICE_BASE_URI)) RIDES_SERVICE_BASE_URI = parts[1];
                        else if (parts[0] == nameof(MAX_SPEAKER_VERIFICATION_ATTEMPTS)) MAX_SPEAKER_VERIFICATION_ATTEMPTS = int.Parse(parts[1]);
                        else if (parts[0] == nameof(_faceRelativeSizeThreshold)) _faceRelativeSizeThreshold = float.Parse(parts[1]);
                        else if (parts[0] == nameof(_emotionScoreThreshold)) _emotionScoreThreshold = float.Parse(parts[1]);
                        else if (parts[0] == nameof(RECORDING_TIME_FOR_VOICE_VERIFICATION_PHRASE)) RECORDING_TIME_FOR_VOICE_VERIFICATION_PHRASE = int.Parse(parts[1]);
                        else if (parts[0] == nameof(LUIS_INTENT_SCORE_THRESHOLD)) LUIS_INTENT_SCORE_THRESHOLD = int.Parse(parts[1]);
                        else if (parts[0] == nameof(_showingSpeechToTextUI)) _showingSpeechToTextUI = bool.Parse(parts[1]);
                        else if (parts[0] == nameof(_showSpeechPartialResult)) _showSpeechPartialResult = bool.Parse(parts[1]);
                        else if (parts[0] == nameof(_SpeechToTextLanguageName)) _SpeechToTextLanguageName = parts[1];
                        else if (parts[0] == nameof(_SpeechToTextInitialSilenceTimeoutInSeconds)) _SpeechToTextInitialSilenceTimeoutInSeconds = int.Parse(parts[1]);
                        else if (parts[0] == nameof(_SpeechToTextBabbleTimeoutInSeconds)) _SpeechToTextBabbleTimeoutInSeconds = int.Parse(parts[1]);
                        else if (parts[0] == nameof(_SpeechToTextEndSilenceTimeoutInSeconds)) _SpeechToTextEndSilenceTimeoutInSeconds = int.Parse(parts[1]);
                        else if (parts[0] == nameof(KIOSK_ID)) KIOSK_ID = int.Parse(parts[1]);
                        else if (parts[0] == nameof(NUMBER_FRAMES_TO_WAIT_BEFORE_STANDBY)) NUMBER_FRAMES_TO_WAIT_BEFORE_STANDBY = int.Parse(parts[1]);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"An error occurred while loading settings: {ex.ToString()}");
                    }
                }
            }

            return true;
        }

        private bool isCtrlKeyPressed = false;
        private SpeechSynthesizer _uwpSynthesizer;

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            Debug.WriteLine($"Key pressed: {e.Key.ToString()}");
            if (e.Key == Windows.System.VirtualKey.Control) isCtrlKeyPressed = true;
        }

        private void Grid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            Debug.WriteLine($"Key released: {e.Key.ToString()}");
            if (e.Key == Windows.System.VirtualKey.Control) isCtrlKeyPressed = false;
            else if (e.Key == Windows.System.VirtualKey.G && isCtrlKeyPressed)
            {
                Debug.WriteLine("Requesting to restart the session");
                _tokenSource?.Cancel();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Requesting to restart the session");
            _tokenSource?.Cancel();
        }

        #endregion
    }
}
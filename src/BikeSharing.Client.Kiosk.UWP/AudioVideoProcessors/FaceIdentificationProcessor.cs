using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face;
using Windows.Graphics.Imaging;
using Windows.Media.FaceAnalysis;

namespace BikeSharing.Client.Kiosk.UWP.AudioVideoProcessors
{
    public class FaceIdentificationProcessor
    {
        private uint _bitmapWidthForCognitiveService;
        private uint _bitmapHeightForCognitiveService;
        private float _faceSizeRatioThreshold;
        private FaceDetector _faceDetector;
        private string _faceApiSubscriptionKey;
        private string _faceApiPersonGroupName;
        private FaceServiceClient _faceServiceClient;
        private BitmapPixelFormat _faceDetectorBitmapPixelFormat;

        public FaceIdentificationProcessor(
            float faceSizeRationThreshold,
            uint bitmapWidthForCognitiveService = 640,
            uint bitmapHeightForCognitiveService = 480)
        {
            _faceSizeRatioThreshold = faceSizeRationThreshold;
            _bitmapWidthForCognitiveService = bitmapWidthForCognitiveService;
            _bitmapHeightForCognitiveService = bitmapHeightForCognitiveService;
            _faceApiSubscriptionKey = App.COGNITIVE_SERVICES_FACE_API_SUBSCRIPTION_KEY;
            _faceApiPersonGroupName = App.COGNITIVE_SERVICES_FACE_API_PERSON_GROUPNAME;
            _faceServiceClient = new FaceServiceClient(_faceApiSubscriptionKey);
            _faceDetectorBitmapPixelFormat = FaceDetector.GetSupportedBitmapPixelFormats().First();
        }

        public async Task InitializeAsync()
        {
            _faceDetector = await FaceDetector.CreateAsync();
        }

        public async Task<FaceDetectionResult> GetDetectedFacesAsync(SoftwareBitmap bitmap)
        {
            var rgba16Bitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Rgba16);
            var convertedBitmap = rgba16Bitmap.BitmapPixelFormat == _faceDetectorBitmapPixelFormat ?
                rgba16Bitmap : SoftwareBitmap.Convert(rgba16Bitmap, _faceDetectorBitmapPixelFormat);
            var detectedFaces = await _faceDetector.DetectFacesAsync(convertedBitmap);
            var facesTooSmall = AreFacesTooSmall(detectedFaces);
            FaceResultType resultType;
            if (detectedFaces == null || !detectedFaces.Any())
                resultType = FaceResultType.NoFaceDetected;
            else if (facesTooSmall)
                resultType = FaceResultType.FaceTooSmall;
            else
                resultType = FaceResultType.Good;
            return new FaceDetectionResult { ResultType = resultType, DetectedFaces = detectedFaces };
        }

        public async Task<FaceIdentificationResult> IdentifyFaceAsync(SoftwareBitmap bitmap, IEnumerable<DetectedFace> detectedFaces)
        {
            try
            {
                var preparedImageStream = await BitmapHelper.PrepareImageForCognitiveServiceApiAsync(
                    bitmap,
                    _bitmapWidthForCognitiveService,
                    _bitmapHeightForCognitiveService);

                // For better performance we use UWP face detector API as a pre-filter so that
                // we only submit requests to the Face API service when there are faces detected
                // by the UWP face detector.
                if (detectedFaces != null && detectedFaces.Any())
                {
                    var faces = await _faceServiceClient.DetectAsync(preparedImageStream);
                    if (faces == null || !faces.Any())
                    {
                        return null;
                    }

                    var identifyResults = await _faceServiceClient.IdentifyAsync(_faceApiPersonGroupName, faces.Select(ff => ff.FaceId).ToArray(), confidenceThreshold: 0.6f);

                    return new FaceIdentificationResult { IdentifyResults = identifyResults };
                }
            }
            catch (FaceAPIException ex)
            {
                Debug.WriteLine($"[Face Identification]: error occured in Identify: {ex.ErrorCode} {ex.ErrorMessage}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Face Identification]: error occured in {ex.ToString()}");
            }

            return null;
        }

        private bool AreFacesTooSmall(IEnumerable<DetectedFace> detectedFaces)
        {
            // Found the face with maximum bounding box;
            var maxFaceArea = detectedFaces?.Select(
                f =>
                {
                    var rectangle = f.FaceBox;
                    return new { Box = f.FaceBox, Area = rectangle.Width * rectangle.Height };
                })
                .OrderByDescending(faceWithArea => faceWithArea.Area)
                .FirstOrDefault();

            var ratio = maxFaceArea != null ? (float)maxFaceArea.Area / _bitmapWidthForCognitiveService / _bitmapHeightForCognitiveService : 0;
            Debug.WriteLine($"[Face Identification]: face relative size: {ratio},  threshold: {_faceSizeRatioThreshold}");
            return ratio < _faceSizeRatioThreshold;
        }
    }
}

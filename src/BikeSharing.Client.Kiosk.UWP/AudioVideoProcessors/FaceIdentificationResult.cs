using Microsoft.ProjectOxford.Face.Contract;
using System.Collections.Generic;
using Windows.Media.FaceAnalysis;

namespace BikeSharing.Client.Kiosk.UWP.AudioVideoProcessors
{
    public enum FaceResultType
    {
        Good,
        FaceTooSmall,
        NoFaceDetected,
    }

    public class FaceIdentificationResult
    {
        public IdentifyResult[] IdentifyResults { get; set; }
    }

    public class FaceDetectionResult
    {
        public FaceResultType ResultType { get; set; }
        public IList<DetectedFace> DetectedFaces { get; set; }

    }
}

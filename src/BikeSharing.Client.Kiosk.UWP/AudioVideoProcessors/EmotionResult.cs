namespace BikeSharing.Client.Kiosk.UWP.AudioVideoProcessors
{
    public class EmotionDetectionResult
    {
        public EmotionType EmotionType { get; private set; }
        public float Score { get; private set; }

        public EmotionDetectionResult(EmotionType emotionType, float score)
        {
            this.EmotionType = emotionType;
            this.Score = score;
        }

        public override string ToString()
        {
            return $"{EmotionType.ToString()} (score {Score.ToString("0.000000")})";
        }
    }
}

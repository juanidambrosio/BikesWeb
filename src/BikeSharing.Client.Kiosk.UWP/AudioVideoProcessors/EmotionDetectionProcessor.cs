using Microsoft.ProjectOxford.Emotion;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace BikeSharing.Client.Kiosk.UWP.AudioVideoProcessors
{
    public class EmotionDetectionProcessor
    {
        public const int IMAGE_WIDTH = 640;
        public const int IMAGE_HEIGHT = 480;

        public async Task<EmotionDetectionResult[]> DetectEmotionAsync(SoftwareBitmap bitmap)
        {
            Stream imageStream = await BitmapHelper.PrepareImageForCognitiveServiceApiAsync(bitmap, IMAGE_WIDTH, IMAGE_HEIGHT);
            
            var client = new EmotionServiceClient(App.COGNITIVE_SERVICES_EMOTION_API_SUBSCRIPTION_KEY);
            var emotions = await client.RecognizeAsync(imageStream);

            EmotionDetectionResult[] result = null;

            if (emotions != null && emotions.Any())
            {
                result = new EmotionDetectionResult[emotions.Length];
                var emotionsWithScore = new EmotionDetectionResult[8];
                for (int i = 0; i < emotions.Length; i++)
                {
                    var emotion = emotions[i];
                    emotionsWithScore[0] = new EmotionDetectionResult(EmotionType.Anger, emotion.Scores.Anger);
                    emotionsWithScore[1] = new EmotionDetectionResult(EmotionType.Contempt, emotion.Scores.Contempt);
                    emotionsWithScore[2] = new EmotionDetectionResult(EmotionType.Disgust, emotion.Scores.Disgust);
                    emotionsWithScore[3] = new EmotionDetectionResult(EmotionType.Fear, emotion.Scores.Fear);
                    emotionsWithScore[4] = new EmotionDetectionResult(EmotionType.Happiness, emotion.Scores.Happiness);
                    emotionsWithScore[5] = new EmotionDetectionResult(EmotionType.Neutral, emotion.Scores.Neutral);
                    emotionsWithScore[6] = new EmotionDetectionResult(EmotionType.Sadness, emotion.Scores.Sadness);
                    emotionsWithScore[7] = new EmotionDetectionResult(EmotionType.Surprise, emotion.Scores.Surprise);

                    var emotionWithHighestScore = emotionsWithScore.OrderByDescending(s => s.Score).First();
                    result[i] = emotionWithHighestScore;
                }
            }

            return result;
        }
    }
}

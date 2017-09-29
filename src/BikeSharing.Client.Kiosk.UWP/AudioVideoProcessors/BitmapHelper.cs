using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace BikeSharing.Client.Kiosk.UWP.AudioVideoProcessors
{
    public static class BitmapHelper
    {
        public static async Task<Stream> PrepareImageForCognitiveServiceApiAsync(SoftwareBitmap softwareBitmap, uint scaledWidth, uint scaledHight)
        {
            var stream = new InMemoryRandomAccessStream();
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            // Set the software bitmap
            encoder.SetSoftwareBitmap(softwareBitmap);

            // Set additional encoding parameters, if needed
            encoder.BitmapTransform.ScaledWidth = scaledWidth;
            encoder.BitmapTransform.ScaledHeight = scaledHight;
            encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;

            await encoder.FlushAsync();
            return stream.AsStreamForRead();
        }

    }
}

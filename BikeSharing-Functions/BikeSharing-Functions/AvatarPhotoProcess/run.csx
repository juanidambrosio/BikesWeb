using System;
using ImageProcessor;
using ImageProcessor.Imaging.Filters.Photo;
using System.IO;

public static async Task Run( byte[] image, string filename, Stream outputBlob, TraceWriter log )
{
    using (ImageFactory imageFactory = new ImageFactory( preserveExifData: true ))
    using (var ms = new MemoryStream( image ))
    {
        imageFactory.Load( ms )
            .Filter( MatrixFilters.GreyScale )
            .Save( outputBlob );
    }

}

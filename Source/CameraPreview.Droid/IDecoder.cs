using System;
using Android.Graphics;

namespace CameraPreview.Droid
{
    public interface IDecoder
    {
        IScanResult Decode(Bitmap image);

        Func<bool> CanProcessImage { get; set; }

        Func<IScanResult, bool> FinishProcessImage { get; set; }

        Action<Exception> HandleExceptionFromProcessImage { get; set; }

        int ImageSizeX { get; }

        int ImageSizeY { get; }

        void ScanningOptionsUpdate(ScanningOptionsBase options);
    }
}
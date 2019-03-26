using System;
using Android.Graphics;

namespace CameraPreview.Droid
{
    public class DefaultDecoderBase : IDecoder
    {
        public Func<bool> CanProcessImage { get; set; }
        public Func<IScanResult, bool> FinishProcessImage { get; set; }
        public Action<Exception> HandleExceptionFromProcessImage { get; set; }

        public int ImageSizeX => 229;

        public int ImageSizeY => 229;

        public virtual IScanResult Decode(Bitmap image)
        {
            return new DefaultScanResult();
        }

        public virtual void ScanningOptionsUpdate(ScanningOptionsBase options)
        {
        }
    }
}


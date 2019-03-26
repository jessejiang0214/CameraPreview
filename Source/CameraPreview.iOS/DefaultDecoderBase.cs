using System;
using System.Threading;
using AVFoundation;
using CoreMedia;
using CoreVideo;
using Foundation;

namespace CameraPreview.iOS
{
    public class DefaultDecoderBase : IDecoder
    {
        public virtual IScanResult Decode(CVPixelBuffer pixelBuffer)
        {
            return new DefaultScanResult();
        }

        public virtual void ScanningOptionsUpdate(ScanningOptionsBase options)
        {
        }
    }
}
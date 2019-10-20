using CoreVideo;

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
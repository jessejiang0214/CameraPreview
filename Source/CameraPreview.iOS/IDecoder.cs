using System.Threading;
using AVFoundation;
using CoreVideo;

namespace CameraPreview.iOS
{
    public interface IDecoder
    {
        IScanResult Decode(CVPixelBuffer pixelBuffer);

        void ScanningOptionsUpdate(ScanningOptionsBase options);
    }

    public interface IOutputRecorder : IAVCaptureVideoDataOutputSampleBufferDelegate
    {
        CancellationTokenSource CancelTokenSource { get; }
    }
}
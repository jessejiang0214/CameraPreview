using System;
using System.Threading;
using AVFoundation;
using CoreMedia;
using CoreVideo;
using Foundation;

namespace CameraPreview.iOS
{
    public class DefaultOutputRecorder : AVCaptureVideoDataOutputSampleBufferDelegate, IOutputRecorder
    {
        private readonly Action<IScanResult> _resultCallback;

        public DefaultOutputRecorder(Action<IScanResult> resultCallback)
        {
            _resultCallback = resultCallback;
        }

        private DateTime _lastAnalysis = DateTime.MinValue;
        private volatile bool _working;
        private volatile bool _wasScanned;

        [Export("captureOutput:didDropSampleBuffer:fromConnection:")]
        public override void DidDropSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer,
            AVCaptureConnection connection)
        {
            Logger.Log("Dropped Sample Buffer");
        }

        public CancellationTokenSource CancelTokenSource { get; set; } = new CancellationTokenSource();

        public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer,
            AVCaptureConnection connection)
        {
            var msSinceLastPreview = (DateTime.UtcNow - _lastAnalysis).TotalMilliseconds;
            var scannerOptions = CameraPreviewSettings.Instance.ScannerOptions;
            if (msSinceLastPreview < scannerOptions.DelayBetweenAnalyzingFrames
                || (_wasScanned && msSinceLastPreview < scannerOptions.DelayBetweenContinuousScans)
                || _working
                || CancelTokenSource.IsCancellationRequested)
            {
                if (msSinceLastPreview < scannerOptions.DelayBetweenAnalyzingFrames)
                    Logger.Log("Too soon between frames", LogLevel.Detail);
                if (_wasScanned && msSinceLastPreview < scannerOptions.DelayBetweenContinuousScans)
                    Logger.Log("Too soon since last scan", LogLevel.Detail);

                sampleBuffer?.Dispose();
                return;
            }

            _wasScanned = false;
            _working = true;
            _lastAnalysis = DateTime.UtcNow;

            try
            {
                // Get the CoreVideo image
                using (var pixelBuffer = sampleBuffer.GetImageBuffer() as CVPixelBuffer)
                {
                    if (pixelBuffer == null) return;

                    // Lock the base address
                    pixelBuffer.Lock(CVPixelBufferLock.ReadOnly); // MAYBE NEEDS READ/WRITE
                    // https://stackoverflow.com/questions/34569750/get-pixel-value-from-cvpixelbufferref-in-swift/42303821
                    var result = CameraPreviewSettings.Instance.Decoder.Decode(pixelBuffer);
                    _resultCallback?.Invoke(result);
                    if (result.Success)
                        _wasScanned = true;

                    pixelBuffer.Unlock(CVPixelBufferLock.ReadOnly);
                }

                //
                // Although this looks innocent "Oh, he is just optimizing this case away"
                // this is incredibly important to call on this callback, because the AVFoundation
                // has a fixed number of buffers and if it runs out of free buffers, it will stop
                // delivering frames. 
                //  
                sampleBuffer.Dispose();
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
            finally
            {
                _working = false;
            }
        }
    }
}
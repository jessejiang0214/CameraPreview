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
        readonly Action<IScanResult> _resultCallback;
        public DefaultOutputRecorder(Action<IScanResult> resultCallback) : base()
        {
            _resultCallback = resultCallback;
        }

        DateTime lastAnalysis = DateTime.MinValue;
        volatile bool working = false;
        volatile bool wasScanned = false;

        [Export("captureOutput:didDropSampleBuffer:fromConnection:")]
        public override void DidDropSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
        {
            Logger.Log("Dropped Sample Buffer");
        }

        public CancellationTokenSource CancelTokenSource { get; set; } = new CancellationTokenSource();


        public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
        {
            var msSinceLastPreview = (DateTime.UtcNow - lastAnalysis).TotalMilliseconds;
            var scannerOptions = CameraPreviewSettings.Instance.ScannerOptions;
            if (msSinceLastPreview < scannerOptions.DelayBetweenAnalyzingFrames
                || (wasScanned && msSinceLastPreview < scannerOptions.DelayBetweenContinuousScans)
                || working
                || CancelTokenSource.IsCancellationRequested)
            {

                if (msSinceLastPreview < scannerOptions.DelayBetweenAnalyzingFrames)
                    Logger.Log("Too soon between frames", LogLevel.Detail);
                if (wasScanned && msSinceLastPreview < scannerOptions.DelayBetweenContinuousScans)
                    Logger.Log("Too soon since last scan", LogLevel.Detail);

                if (sampleBuffer != null)
                {
                    sampleBuffer.Dispose();
                    sampleBuffer = null;
                }
                return;
            }

            wasScanned = false;
            working = true;
            lastAnalysis = DateTime.UtcNow;

            try
            {
                // Get the CoreVideo image
                using (var pixelBuffer = sampleBuffer.GetImageBuffer() as CVPixelBuffer)
                {
                    // Lock the base address
                    pixelBuffer.Lock(CVPixelBufferLock.ReadOnly); // MAYBE NEEDS READ/WRITE

                    IScanResult result = CameraPreviewSettings.Instance.Decoder.Decode(pixelBuffer);
                    _resultCallback?.Invoke(result);
                    if (result.Success)
                        wasScanned = true;

                    pixelBuffer.Unlock(CVPixelBufferLock.ReadOnly);
                }

                //
                // Although this looks innocent "Oh, he is just optimizing this case away"
                // this is incredibly important to call on this callback, because the AVFoundation
                // has a fixed number of buffers and if it runs out of free buffers, it will stop
                // delivering frames. 
                //  
                sampleBuffer.Dispose();
                sampleBuffer = null;

            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
            finally
            {
                working = false;
            }

        }
    }

}

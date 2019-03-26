using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using AVFoundation;
using CoreFoundation;
using CoreGraphics;
using CoreMedia;
using CoreVideo;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace CameraPreview.iOS
{
    public class CPScannerView : UIView
    {
        public delegate void ScannerSetupCompleteDelegate();
        public event ScannerSetupCompleteDelegate OnScannerSetupComplete;

        AVCaptureSession session;
        AVCaptureVideoPreviewLayer previewLayer;
        AVCaptureVideoDataOutput output;
        IOutputRecorder outputRecorder;
        DispatchQueue queue;
        Action<IScanResult> resultCallback;
        volatile bool stopped = true;

        UIView layerView;

        public string CancelButtonText { get; set; }
        public string FlashButtonText { get; set; }

        bool shouldRotatePreviewBuffer = false;

        void Setup(CGRect frame)
        {
            var started = DateTime.UtcNow;
            var total = DateTime.UtcNow - started;
            Logger.Log($"CPScannerView.Setup() took {total.TotalMilliseconds} ms.");
        }

        bool analyzing = true;

        bool SetupCaptureSession()
        {
            if (CameraPreviewSettings.Instance.Decoder == null)
                return false;

            var started = DateTime.UtcNow;

            var availableResolutions = new List<CameraResolution>();

            var consideredResolutions = new Dictionary<NSString, CameraResolution> {
                { AVCaptureSession.Preset352x288, new CameraResolution   { Width = 352,  Height = 288 } },
                { AVCaptureSession.PresetMedium, new CameraResolution    { Width = 480,  Height = 360 } },  //480x360
                { AVCaptureSession.Preset640x480, new CameraResolution   { Width = 640,  Height = 480 } },
                { AVCaptureSession.Preset1280x720, new CameraResolution  { Width = 1280, Height = 720 } },
                { AVCaptureSession.Preset1920x1080, new CameraResolution { Width = 1920, Height = 1080 } }
            };

            // configure the capture session for low resolution, change this if your code
            // can cope with more data or volume
            session = new AVCaptureSession()
            {
                SessionPreset = AVCaptureSession.Preset640x480
            };

            // create a device input and attach it to the session
            //          var captureDevice = AVCaptureDevice.DefaultDeviceWithMediaType (AVMediaType.Video);
            AVCaptureDevice captureDevice = null;
            var devices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
            foreach (var device in devices)
            {
                captureDevice = device;
                if (CameraPreviewSettings.Instance.ScannerOptions.UseFrontCameraIfAvailable.HasValue &&
                    CameraPreviewSettings.Instance.ScannerOptions.UseFrontCameraIfAvailable.Value &&
                    device.Position == AVCaptureDevicePosition.Front)

                    break; //Front camera successfully set
                else if (device.Position == AVCaptureDevicePosition.Back &&
                       (!CameraPreviewSettings.Instance.ScannerOptions.UseFrontCameraIfAvailable.HasValue
                        || !CameraPreviewSettings.Instance.ScannerOptions.UseFrontCameraIfAvailable.Value))
                    break; //Back camera succesfully set
            }
            if (captureDevice == null)
            {
                Console.WriteLine("No captureDevice - this won't work on the simulator, try a physical device");
                return false;
            }

            CameraResolution resolution = null;

            // Find resolution
            // Go through the resolutions we can even consider
            foreach (var cr in consideredResolutions)
            {
                // Now check to make sure our selected device supports the resolution
                // so we can add it to the list to pick from
                if (captureDevice.SupportsAVCaptureSessionPreset(cr.Key))
                    availableResolutions.Add(cr.Value);
            }

            resolution = CameraPreviewSettings.Instance.ScannerOptions.GetResolution(availableResolutions);

            // See if the user selected a resolution
            if (resolution != null)
            {
                // Now get the preset string from the resolution chosen
                var preset = (from c in consideredResolutions
                              where c.Value.Width == resolution.Width
                                && c.Value.Height == resolution.Height
                              select c.Key).FirstOrDefault();

                // If we found a matching preset, let's set it on the session
                if (!string.IsNullOrEmpty(preset))
                    session.SessionPreset = preset;
            }

            var input = AVCaptureDeviceInput.FromDevice(captureDevice);
            if (input == null)
            {
                Console.WriteLine("No input - this won't work on the simulator, try a physical device");
                return false;
            }
            else
                session.AddInput(input);


            var startedAVPreviewLayerAlloc = PerformanceCounter.Start();

            previewLayer = new AVCaptureVideoPreviewLayer(session);

            PerformanceCounter.Stop(startedAVPreviewLayerAlloc, "Alloc AVCaptureVideoPreviewLayer took {0} ms.");

            var perf2 = PerformanceCounter.Start();

#if __UNIFIED__
            previewLayer.VideoGravity = AVLayerVideoGravity.ResizeAspectFill;
#else
            previewLayer.LayerVideoGravity = AVLayerVideoGravity.ResizeAspectFill;
#endif
            previewLayer.Frame = new CGRect(0, 0, this.Frame.Width, this.Frame.Height);
            previewLayer.Position = new CGPoint(this.Layer.Bounds.Width / 2, (this.Layer.Bounds.Height / 2));

            layerView = new UIView(new CGRect(0, 0, this.Frame.Width, this.Frame.Height));
            layerView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            layerView.Layer.AddSublayer(previewLayer);

            this.AddSubview(layerView);

            ResizePreview(UIApplication.SharedApplication.StatusBarOrientation);


            PerformanceCounter.Stop(perf2, "PERF: Setting up layers took {0} ms");

            var perf3 = PerformanceCounter.Start();

            session.StartRunning();

            PerformanceCounter.Stop(perf3, "PERF: session.StartRunning() took {0} ms");

            var perf4 = PerformanceCounter.Start();

            var videoSettings = NSDictionary.FromObjectAndKey(new NSNumber((int)CVPixelFormatType.CV32BGRA),
                CVPixelBuffer.PixelFormatTypeKey);


            // create a VideoDataOutput and add it to the sesion
            output = new AVCaptureVideoDataOutput
            {
                WeakVideoSettings = videoSettings
            };

            // configure the output
            queue = new DispatchQueue("CamerPreviewView"); // (Guid.NewGuid().ToString());
            outputRecorder = new DefaultOutputRecorder(resultCallback);
            output.AlwaysDiscardsLateVideoFrames = true;
            output.SetSampleBufferDelegateQueue(outputRecorder, queue);

            PerformanceCounter.Stop(perf4, "PERF: SetupCamera Finished.  Took {0} ms.");

            session.AddOutput(output);
            //session.StartRunning ();


            var perf5 = PerformanceCounter.Start();

            NSError err = null;
            if (captureDevice.LockForConfiguration(out err))
            {
                if (captureDevice.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
                    captureDevice.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
                else if (captureDevice.IsFocusModeSupported(AVCaptureFocusMode.AutoFocus))
                    captureDevice.FocusMode = AVCaptureFocusMode.AutoFocus;

                if (captureDevice.IsExposureModeSupported(AVCaptureExposureMode.ContinuousAutoExposure))
                    captureDevice.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
                else if (captureDevice.IsExposureModeSupported(AVCaptureExposureMode.AutoExpose))
                    captureDevice.ExposureMode = AVCaptureExposureMode.AutoExpose;

                if (captureDevice.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance))
                    captureDevice.WhiteBalanceMode = AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance;
                else if (captureDevice.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.AutoWhiteBalance))
                    captureDevice.WhiteBalanceMode = AVCaptureWhiteBalanceMode.AutoWhiteBalance;

                if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0) && captureDevice.AutoFocusRangeRestrictionSupported)
                    captureDevice.AutoFocusRangeRestriction = AVCaptureAutoFocusRangeRestriction.Near;

                if (captureDevice.FocusPointOfInterestSupported)
                    captureDevice.FocusPointOfInterest = new PointF(0.5f, 0.5f);

                if (captureDevice.ExposurePointOfInterestSupported)
                    captureDevice.ExposurePointOfInterest = new PointF(0.5f, 0.5f);

                captureDevice.UnlockForConfiguration();
            }
            else
                Logger.Log("Failed to Lock for Config: " + err.Description);

            PerformanceCounter.Stop(perf5, "PERF: Setup Focus in {0} ms.");

            return true;
        }

        public void DidRotate(UIInterfaceOrientation orientation)
        {
            ResizePreview(orientation);

            this.LayoutSubviews();

            //          if (overlayView != null)
            //  overlayView.LayoutSubviews ();
        }

        public void ResizePreview(UIInterfaceOrientation orientation)
        {
            shouldRotatePreviewBuffer = orientation == UIInterfaceOrientation.Portrait || orientation == UIInterfaceOrientation.PortraitUpsideDown;

            if (previewLayer == null)
                return;

            previewLayer.Frame = new CGRect(0, 0, this.Frame.Width, this.Frame.Height);

            if (previewLayer.RespondsToSelector(new Selector("connection")) && previewLayer.Connection != null)
            {
                switch (orientation)
                {
                    case UIInterfaceOrientation.LandscapeLeft:
                        previewLayer.Connection.VideoOrientation = AVCaptureVideoOrientation.LandscapeLeft;
                        break;
                    case UIInterfaceOrientation.LandscapeRight:
                        previewLayer.Connection.VideoOrientation = AVCaptureVideoOrientation.LandscapeRight;
                        break;
                    case UIInterfaceOrientation.Portrait:
                        previewLayer.Connection.VideoOrientation = AVCaptureVideoOrientation.Portrait;
                        break;
                    case UIInterfaceOrientation.PortraitUpsideDown:
                        previewLayer.Connection.VideoOrientation = AVCaptureVideoOrientation.PortraitUpsideDown;
                        break;
                }
            }
        }

        public void StartScanning(Action<IScanResult> scanResultHandler, ScanningOptionsBase options = null)
        {
            if (!stopped)
                return;

            stopped = false;

            var perf = PerformanceCounter.Start();

            Setup(this.Frame);

            CameraPreviewSettings.Instance.SetScannerOptions(options);
            this.resultCallback = scanResultHandler;

            Logger.Log("StartScanning");

            this.InvokeOnMainThread(() =>
            {
                if (!SetupCaptureSession())
                {
                    //Setup 'simulated' view:
                    Logger.Log("Capture Session FAILED");

                }

                if (Runtime.Arch == Arch.SIMULATOR)
                {
                    var simView = new UIView(new CGRect(0, 0, this.Frame.Width, this.Frame.Height));
                    simView.BackgroundColor = UIColor.LightGray;
                    simView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
                    this.InsertSubview(simView, 0);

                }
            });

            if (!analyzing)
                analyzing = true;

            PerformanceCounter.Stop(perf, "PERF: StartScanning() Took {0} ms.");

            var evt = this.OnScannerSetupComplete;
            if (evt != null)
                evt();
        }

        public void StopScanning()
        {
            if (stopped)
                return;

            Console.WriteLine("Stopping...");

            if (outputRecorder != null)
                outputRecorder.CancelTokenSource.Cancel();

            //Try removing all existing outputs prior to closing the session
            try
            {
                while (session.Outputs.Length > 0)
                    session.RemoveOutput(session.Outputs[0]);
            }
            catch { }

            //Try to remove all existing inputs prior to closing the session
            try
            {
                while (session.Inputs.Length > 0)
                    session.RemoveInput(session.Inputs[0]);
            }
            catch { }

            if (session.Running)
                session.StopRunning();

            stopped = true;
        }

        public void PauseAnalysis()
        {
            analyzing = false;
        }

        public void ResumeAnalysis()
        {
            analyzing = true;
        }

        public bool IsAnalyzing { get { return analyzing; } }

    }
}

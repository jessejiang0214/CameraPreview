using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AVFoundation;
using CoreFoundation;
using CoreGraphics;
using CoreVideo;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace CameraPreview.iOS
{
    public class CpScannerView : UIView
    {
        public delegate void ScannerSetupCompleteDelegate();

        public event ScannerSetupCompleteDelegate OnScannerSetupComplete;

        private AVCaptureSession _session;
        private AVCaptureVideoPreviewLayer _previewLayer;
        private AVCaptureVideoDataOutput _output;
        private IOutputRecorder _outputRecorder;
        private DispatchQueue _queue;
        private Action<IScanResult> _resultCallback;
        private volatile bool _stopped = true;

        private UIView _layerView;

        public string CancelButtonText { get; set; }
        public string FlashButtonText { get; set; }

        private bool _shouldRotatePreviewBuffer = false;

        private static void Setup(CGRect frame)
        {
            var started = DateTime.UtcNow;
            var total = DateTime.UtcNow - started;
            Logger.Log($"CPScannerView.Setup() took {total.TotalMilliseconds} ms.");
        }

        private bool _analyzing = true;

        private bool SetupCaptureSession()
        {
            if (CameraPreviewSettings.Instance.Decoder == null)
                return false;

            var started = DateTime.UtcNow;

            var availableResolutions = new List<CameraResolution>();

            var consideredResolutions = new Dictionary<NSString, CameraResolution>
            {
                {AVCaptureSession.Preset352x288, new CameraResolution {Width = 352, Height = 288}},
                {AVCaptureSession.PresetMedium, new CameraResolution {Width = 480, Height = 360}}, //480x360
                {AVCaptureSession.Preset640x480, new CameraResolution {Width = 640, Height = 480}},
                {AVCaptureSession.Preset1280x720, new CameraResolution {Width = 1280, Height = 720}},
                {AVCaptureSession.Preset1920x1080, new CameraResolution {Width = 1920, Height = 1080}}
            };

            // configure the capture session for low resolution, change this if your code
            // can cope with more data or volume
            _session = new AVCaptureSession()
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
                    break; //Back camera successfully set
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
                    _session.SessionPreset = preset;
            }

            var input = AVCaptureDeviceInput.FromDevice(captureDevice);
            if (input == null)
            {
                Console.WriteLine("No input - this won't work on the simulator, try a physical device");
                return false;
            }
            else
                _session.AddInput(input);


            var startedAvPreviewLayerAlloc = PerformanceCounter.Start();

            _previewLayer = new AVCaptureVideoPreviewLayer(_session);

            PerformanceCounter.Stop(startedAvPreviewLayerAlloc, "Alloc AVCaptureVideoPreviewLayer took {0} ms.");

            var perf2 = PerformanceCounter.Start();

#if __UNIFIED__
            _previewLayer.VideoGravity = AVLayerVideoGravity.ResizeAspectFill;
#else
            previewLayer.LayerVideoGravity = AVLayerVideoGravity.ResizeAspectFill;
#endif
            _previewLayer.Frame = new CGRect(0, 0, this.Frame.Width, this.Frame.Height);
            _previewLayer.Position = new CGPoint(this.Layer.Bounds.Width / 2, (this.Layer.Bounds.Height / 2));

            _layerView = new UIView(new CGRect(0, 0, this.Frame.Width, this.Frame.Height))
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight
            };
            _layerView.Layer.AddSublayer(_previewLayer);

            this.AddSubview(_layerView);

            ResizePreview(UIApplication.SharedApplication.StatusBarOrientation);


            PerformanceCounter.Stop(perf2, "PERF: Setting up layers took {0} ms");

            var perf3 = PerformanceCounter.Start();

            _session.StartRunning();

            PerformanceCounter.Stop(perf3, "PERF: session.StartRunning() took {0} ms");

            var perf4 = PerformanceCounter.Start();

            var videoSettings = NSDictionary.FromObjectAndKey(new NSNumber((int) CVPixelFormatType.CV32BGRA),
                CVPixelBuffer.PixelFormatTypeKey);


            // create a VideoDataOutput and add it to the sesion
            _output = new AVCaptureVideoDataOutput
            {
                WeakVideoSettings = videoSettings
            };

            // configure the output
            _queue = new DispatchQueue("CamerPreviewView"); // (Guid.NewGuid().ToString());
            _outputRecorder = new DefaultOutputRecorder(_resultCallback);
            _output.AlwaysDiscardsLateVideoFrames = true;
            _output.SetSampleBufferDelegateQueue(_outputRecorder, _queue);

            PerformanceCounter.Stop(perf4, "PERF: SetupCamera Finished.  Took {0} ms.");

            _session.AddOutput(_output);
            //session.StartRunning ();


            var perf5 = PerformanceCounter.Start();

            if (captureDevice.LockForConfiguration(out var err))
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
            _shouldRotatePreviewBuffer = orientation == UIInterfaceOrientation.Portrait ||
                                         orientation == UIInterfaceOrientation.PortraitUpsideDown;

            if (_previewLayer == null)
                return;

            _previewLayer.Frame = new CGRect(0, 0, this.Frame.Width, this.Frame.Height);

            if (_previewLayer.RespondsToSelector(new Selector("connection")) && _previewLayer.Connection != null)
            {
                switch (orientation)
                {
                    case UIInterfaceOrientation.LandscapeLeft:
                        _previewLayer.Connection.VideoOrientation = AVCaptureVideoOrientation.LandscapeLeft;
                        break;
                    case UIInterfaceOrientation.LandscapeRight:
                        _previewLayer.Connection.VideoOrientation = AVCaptureVideoOrientation.LandscapeRight;
                        break;
                    case UIInterfaceOrientation.Portrait:
                        _previewLayer.Connection.VideoOrientation = AVCaptureVideoOrientation.Portrait;
                        break;
                    case UIInterfaceOrientation.PortraitUpsideDown:
                        _previewLayer.Connection.VideoOrientation = AVCaptureVideoOrientation.PortraitUpsideDown;
                        break;
                }
            }
        }

        public void StartScanning(Action<IScanResult> scanResultHandler, ScanningOptionsBase options = null)
        {
            if (!_stopped)
                return;

            _stopped = false;

            var perf = PerformanceCounter.Start();

            Setup(this.Frame);

            CameraPreviewSettings.Instance.SetScannerOptions(options);
            this._resultCallback = scanResultHandler;

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
                    var simView = new UIView(new CGRect(0, 0, this.Frame.Width, this.Frame.Height))
                    {
                        BackgroundColor = UIColor.LightGray,
                        AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight
                    };
                    this.InsertSubview(simView, 0);

                }
            });

            if (!_analyzing)
                _analyzing = true;

            PerformanceCounter.Stop(perf, "PERF: StartScanning() Took {0} ms.");

            var evt = this.OnScannerSetupComplete;
            evt?.Invoke();
        }

        public void StopScanning()
        {
            if (_stopped)
                return;

            Console.WriteLine("Stopping...");

            _outputRecorder?.CancelTokenSource.Cancel();

            //Try removing all existing outputs prior to closing the session
            try
            {
                while (_session.Outputs.Length > 0)
                    _session.RemoveOutput(_session.Outputs[0]);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
            }

            //Try to remove all existing inputs prior to closing the session
            try
            {
                while (_session.Inputs.Length > 0)
                    _session.RemoveInput(_session.Inputs[0]);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
            }

            if (_session.Running)
                _session.StopRunning();

            _stopped = true;
        }

        public void PauseAnalysis()
        {
            _analyzing = false;
        }

        public void ResumeAnalysis()
        {
            _analyzing = true;
        }

        public bool IsAnalyzing => _analyzing;
    }
}
using System.ComponentModel;
using CameraPreview;
using CameraPreview.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ScannerView), typeof(CPScannerViewRenderer))]

namespace CameraPreview.iOS
{
    public class CPScannerViewRenderer : ViewRenderer<ScannerView, CpScannerView>
    {
        protected ScannerView FormsView;
        protected CpScannerView PlatformView;

        protected override void OnElementChanged(ElementChangedEventArgs<ScannerView> e)
        {
            AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

            FormsView = Element;

            if (PlatformView == null)
            {

                PlatformView = new CpScannerView
                {
                    AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight
                };

                base.SetNativeControl(PlatformView);

                if (FormsView.IsScanning)
                    PlatformView.StartScanning(FormsView.RaiseScanResult, FormsView.Options);

                if (!FormsView.IsAnalyzing)
                    PlatformView.PauseAnalysis();
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (PlatformView == null)
                return;

            switch (e.PropertyName)
            {
                case nameof(ScannerView.IsScanning):
                    if (FormsView.IsScanning)
                        PlatformView.StartScanning(FormsView.RaiseScanResult, FormsView.Options);
                    else
                        PlatformView.StopScanning();
                    break;
                case nameof(ScannerView.IsAnalyzing):
                    if (FormsView.IsAnalyzing)
                        PlatformView.ResumeAnalysis();
                    else
                        PlatformView.PauseAnalysis();
                    break;
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            // Find the best guess at current orientation
            var o = UIApplication.SharedApplication.StatusBarOrientation;
            if (ViewController != null)
                o = ViewController.InterfaceOrientation;

            // Tell the native view to rotate
            PlatformView.DidRotate(o);
        }
    }
}


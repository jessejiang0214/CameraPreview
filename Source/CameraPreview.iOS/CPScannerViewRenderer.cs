using System;
using System.ComponentModel;
using CameraPreview;
using CameraPreview.iOS;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ScannerView), typeof(CPScannerViewRenderer))]
namespace CameraPreview.iOS
{
    public class CPScannerViewRenderer : ViewRenderer<ScannerView, CPScannerView>
    {
        protected ScannerView formsView;
        protected CPScannerView nativeView;

        protected override void OnElementChanged(ElementChangedEventArgs<ScannerView> e)
        {
            AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

            formsView = Element;

            if (nativeView == null)
            {

                nativeView = new CPScannerView();
                nativeView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

                base.SetNativeControl(nativeView);

                if (formsView.IsScanning)
                    nativeView.StartScanning(formsView.RaiseScanResult, formsView.Options);

                if (!formsView.IsAnalyzing)
                    nativeView.PauseAnalysis();
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (nativeView == null)
                return;

            switch (e.PropertyName)
            {
                case nameof(ScannerView.IsScanning):
                    if (formsView.IsScanning)
                        nativeView.StartScanning(formsView.RaiseScanResult, formsView.Options);
                    else
                        nativeView.StopScanning();
                    break;
                case nameof(ScannerView.IsAnalyzing):
                    if (formsView.IsAnalyzing)
                        nativeView.ResumeAnalysis();
                    else
                        nativeView.PauseAnalysis();
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
            nativeView.DidRotate(o);
        }
    }
}


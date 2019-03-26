using System;
using CameraPreview;
using Xamarin.Forms;
using ZXing.Net.Xamarin.Forms;

namespace Sample
{
    public class ScannerPage : ContentPage
    {
        ScannerView _scannerView;
        ZXingOverlay defaultOverlay = null;

        public ScannerPage(ScanningOptionsBase options = null, View customOverlay = null) : base()
        {
            _scannerView = new ScannerView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Options = options,
            };

            _scannerView.SetBinding(ScannerView.IsAnalyzingProperty, new Binding(nameof(IsAnalyzing)));
            _scannerView.SetBinding(ScannerView.IsScanningProperty, new Binding(nameof(IsScanning)));
            _scannerView.SetBinding(ScannerView.ResultProperty, new Binding(nameof(Result)));

            _scannerView.OnScanResult += (result) =>
            {
                this.OnScanResult?.Invoke(result);
            };

            if (customOverlay == null)
            {
                defaultOverlay = new ZXingOverlay() { };

                defaultOverlay.SetBinding(ZXingOverlay.TopTextProperty, new Binding(nameof(DefaultOverlayTopText)));
                defaultOverlay.SetBinding(ZXingOverlay.BottomTextProperty, new Binding(nameof(DefaultOverlayBottomText)));

                DefaultOverlayTopText = "Hold your phone up to the barcode";
                DefaultOverlayBottomText = "Scanning will happen automatically";

                Overlay = defaultOverlay;
            }
            else
            {
                Overlay = customOverlay;
            }

            var grid = new Grid
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            grid.Children.Add(_scannerView);
            grid.Children.Add(Overlay);

            // The root page of your application
            Content = grid;
        }

        #region Default Overlay Properties

        public static readonly BindableProperty DefaultOverlayTopTextProperty =
            BindableProperty.Create(nameof(DefaultOverlayTopText), typeof(string), typeof(ScannerPage), string.Empty);
        public string DefaultOverlayTopText
        {
            get { return (string)GetValue(DefaultOverlayTopTextProperty); }
            set { SetValue(DefaultOverlayTopTextProperty, value); }
        }

        public static readonly BindableProperty DefaultOverlayBottomTextProperty =
            BindableProperty.Create(nameof(DefaultOverlayBottomText), typeof(string), typeof(ScannerPage), string.Empty);
        public string DefaultOverlayBottomText
        {
            get { return (string)GetValue(DefaultOverlayBottomTextProperty); }
            set { SetValue(DefaultOverlayBottomTextProperty, value); }
        }

        #endregion

        public delegate void ScanResultDelegate(IScanResult result);
        public event ScanResultDelegate OnScanResult;

        public View Overlay
        {
            get;
            private set;
        }

        #region Functions

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _scannerView.IsScanning = true;
        }

        protected override void OnDisappearing()
        {
            _scannerView.IsScanning = false;

            base.OnDisappearing();
        }

        public void PauseAnalysis()
        {
            if (_scannerView != null)
                _scannerView.IsAnalyzing = false;
        }

        public void ResumeAnalysis()
        {
            if (_scannerView != null)
                _scannerView.IsAnalyzing = true;
        }


        #endregion
        public static readonly BindableProperty IsAnalyzingProperty =
            BindableProperty.Create(nameof(IsAnalyzing), typeof(bool), typeof(ScannerPage), false);
        public bool IsAnalyzing
        {
            get { return (bool)GetValue(IsAnalyzingProperty); }
            set { SetValue(IsAnalyzingProperty, value); }
        }

        public static readonly BindableProperty IsScanningProperty =
            BindableProperty.Create(nameof(IsScanning), typeof(bool), typeof(ScannerPage), false);
        public bool IsScanning
        {
            get { return (bool)GetValue(IsScanningProperty); }
            set { SetValue(IsScanningProperty, value); }
        }

        public static readonly BindableProperty ResultProperty =
            BindableProperty.Create(nameof(Result), typeof(IScanResult), typeof(ScannerPage), default(IScanResult));
        public IScanResult Result
        {
            get { return (IScanResult)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }
    }
}


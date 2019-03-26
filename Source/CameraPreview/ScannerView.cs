using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace CameraPreview
{
    public class ScannerView : View
    {
        public delegate void ScanResultDelegate(IScanResult result);
        public event ScanResultDelegate OnScanResult;

        public ScannerView()
        {
            VerticalOptions = LayoutOptions.FillAndExpand;
            HorizontalOptions = LayoutOptions.FillAndExpand;
        }

        public void RaiseScanResult(IScanResult result)
        {
            Result = result;
            OnScanResult?.Invoke(Result);
            ScanResultCommand?.Execute(Result);
        }

        public static readonly BindableProperty OptionsProperty =
            BindableProperty.Create(nameof(Options), typeof(ScanningOptionsBase), typeof(ScannerView), ScanningOptionsBase.Default);

        public ScanningOptionsBase Options
        {
            get { return (ScanningOptionsBase)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }

        public static readonly BindableProperty IsScanningProperty =
            BindableProperty.Create(nameof(IsScanning), typeof(bool), typeof(ScannerView), false);

        public bool IsScanning
        {
            get { return (bool)GetValue(IsScanningProperty); }
            set { SetValue(IsScanningProperty, value); }
        }

        public static readonly BindableProperty IsAnalyzingProperty =
            BindableProperty.Create(nameof(IsAnalyzing), typeof(bool), typeof(ScannerView), true);

        public bool IsAnalyzing
        {
            get { return (bool)GetValue(IsAnalyzingProperty); }
            set { SetValue(IsAnalyzingProperty, value); }
        }

        public static readonly BindableProperty ResultProperty =
            BindableProperty.Create(nameof(Result), typeof(IScanResult), typeof(ScannerView), default(IScanResult));
        public IScanResult Result
        {
            get { return (IScanResult)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public static readonly BindableProperty ScanResultCommandProperty =
            BindableProperty.Create(nameof(ScanResultCommand), typeof(ICommand), typeof(ScannerView), default(ICommand));
        public ICommand ScanResultCommand
        {
            get { return (ICommand)GetValue(ScanResultCommandProperty); }
            set { SetValue(ScanResultCommandProperty, value); }
        }
    }
}


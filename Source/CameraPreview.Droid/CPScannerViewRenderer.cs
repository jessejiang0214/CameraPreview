using System.ComponentModel;
using Android.Content;
using Android.Views;
using CameraPreview;
using CameraPreview.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ScannerView), typeof(CPScannerViewRenderer))]

namespace CameraPreview.Droid
{
    public class CPScannerViewRenderer : ViewRenderer<ScannerView, CPSurfaceView>
    {
        public CPScannerViewRenderer(Context context) : base(context)
        {
        }

        protected ScannerView FormsView;
        protected CPSurfaceView PlatformView;
        private CameraAnalyzer _cameraAnalyzer;

        protected override void OnElementChanged(ElementChangedEventArgs<ScannerView> e)
        {
            base.OnElementChanged(e);
            FormsView = Element;

            if (PlatformView == null)
            {
                PlatformView = new CPSurfaceView(this.Context)
                {
                    LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
                };
                _cameraAnalyzer = PlatformView.CameraAnalyzer;

                PlatformView.SurfaceTextureAvailable += NativeSurface_SurfaceTextureAvailable;
                PlatformView.SurfaceTextureDestroyed += NativeSurface_SurfaceTextureDestroyed;
                PlatformView.SurfaceTextureSizeChanged += NativeSurface_SurfaceTextureSizeChanged;
                base.SetNativeControl(PlatformView);

                if (FormsView.IsScanning)
                    PlatformView.StartScanning(FormsView.RaiseScanResult, FormsView.Options);

                if (!FormsView.IsAnalyzing)
                    PlatformView.PauseAnalysis();
            }
        }

        private void NativeSurface_SurfaceTextureAvailable(object sender,
            TextureView.SurfaceTextureAvailableEventArgs e)
        {
            _cameraAnalyzer.SetupCamera(e.Width, e.Height);
        }

        private void NativeSurface_SurfaceTextureDestroyed(object sender,
            TextureView.SurfaceTextureDestroyedEventArgs e)
        {
            _cameraAnalyzer.ShutdownCamera();
        }

        private void NativeSurface_SurfaceTextureSizeChanged(object sender,
            TextureView.SurfaceTextureSizeChangedEventArgs e)
        {
            _cameraAnalyzer.ConfigureTransform(e.Width, e.Height);
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
    }
}
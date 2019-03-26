using System;
using System.ComponentModel;
using System.Threading.Tasks;
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

        protected ScannerView formsView;

        protected CPSurfaceView nativeSurface;
        CameraAnalyzer _cameraAnalyzer;
        protected override void OnElementChanged(ElementChangedEventArgs<ScannerView> e)
        {
            base.OnElementChanged(e);

            formsView = Element;

            if (nativeSurface == null)
            {

                nativeSurface = new CPSurfaceView(this.Context);
                nativeSurface.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                _cameraAnalyzer = nativeSurface.CameraAnalyzer;

                nativeSurface.SurfaceTextureAvailable += NativeSurface_SurfaceTextureAvailable;
                nativeSurface.SurfaceTextureDestroyed += NativeSurface_SurfaceTextureDestroyed;
                nativeSurface.SurfaceTextureSizeChanged += NativeSurface_SurfaceTextureSizeChanged;
                base.SetNativeControl(nativeSurface);

                if (formsView.IsScanning)
                    nativeSurface.StartScanning(formsView.RaiseScanResult, formsView.Options);

                if (!formsView.IsAnalyzing)
                    nativeSurface.PauseAnalysis();
            }
        }

        void NativeSurface_SurfaceTextureAvailable(object sender, TextureView.SurfaceTextureAvailableEventArgs e)
        {
            _cameraAnalyzer.SetupCamera(e.Width, e.Height);
        }

        void NativeSurface_SurfaceTextureDestroyed(object sender, TextureView.SurfaceTextureDestroyedEventArgs e)
        {
            _cameraAnalyzer.ShutdownCamera();
        }

        void NativeSurface_SurfaceTextureSizeChanged(object sender, TextureView.SurfaceTextureSizeChangedEventArgs e)
        {
            _cameraAnalyzer.ConfigureTransform(e.Width, e.Height);
        }


        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (nativeSurface == null)
                return;

            switch (e.PropertyName)
            {
                case nameof(ScannerView.IsScanning):
                    if (formsView.IsScanning)
                        nativeSurface.StartScanning(formsView.RaiseScanResult, formsView.Options);
                    else
                        nativeSurface.StopScanning();
                    break;
                case nameof(ScannerView.IsAnalyzing):
                    if (formsView.IsAnalyzing)
                        nativeSurface.ResumeAnalysis();
                    else
                        nativeSurface.PauseAnalysis();
                    break;
            }
        }
    }
}



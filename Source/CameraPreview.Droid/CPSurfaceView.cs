using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace CameraPreview.Droid
{
    public class CPSurfaceView : TextureView
    {
        private int _ratioWidth = 0;
        private int _ratioHeight = 0;

        public CPSurfaceView(Context context)
            : this(context, null)
        {
            Init();
        }
        public CPSurfaceView(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {

        }
        public CPSurfaceView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {

        }

        private void Init()
        {
            if (_cameraAnalyzer == null)
                _cameraAnalyzer = new CameraAnalyzer(this);

            _cameraAnalyzer.ResumeAnalysis();

        }

        public void SetAspectRatio(int width, int height)
        {
            if (width == 0 || height == 0)
                throw new ArgumentException("Size cannot be negative.");
            _ratioWidth = width;
            _ratioHeight = height;
            RequestLayout();
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            int width = MeasureSpec.GetSize(widthMeasureSpec);
            int height = MeasureSpec.GetSize(heightMeasureSpec);
            if (0 == _ratioWidth || 0 == _ratioHeight)
            {
                SetMeasuredDimension(width, height);
            }
            else
            {
                if (width < (float)height * _ratioWidth / (float)_ratioHeight)
                {
                    SetMeasuredDimension(width, width * _ratioHeight / _ratioWidth);
                }
                else
                {
                    SetMeasuredDimension(height * _ratioWidth / _ratioHeight, height);
                }
            }
        }

        public void StartScanning(Action<IScanResult> scanResultCallback, ScanningOptionsBase options = null)
        {
            //fix Android 7 bug: camera freezes because surfacedestroyed function isn't always called correct, the old surfaceview was still visible.
            this.Visibility = ViewStates.Visible;

            CameraPreviewSettings.Instance.SetScannerOptions(options);

            _cameraAnalyzer.ResultFound += (sender, result) =>
            {
                scanResultCallback?.Invoke(result);
            };
            _cameraAnalyzer.ResumeAnalysis();
        }

        public void StopScanning()
        {
            _cameraAnalyzer.ShutdownCamera();
            //fix Android 7 bug: camera freezes because surfacedestroyed function isn't always called correct, the old surfaceview was still visible.
            this.Visibility = ViewStates.Gone;
        }

        public void PauseAnalysis()
        {
            _cameraAnalyzer.PauseAnalysis();
        }

        public void ResumeAnalysis()
        {
            _cameraAnalyzer.ResumeAnalysis();
        }

        public bool IsAnalyzing => _cameraAnalyzer.IsAnalyzing;

        private CameraAnalyzer _cameraAnalyzer;
        public CameraAnalyzer CameraAnalyzer => _cameraAnalyzer;

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();

            Init();
        }

        protected override void OnWindowVisibilityChanged(ViewStates visibility)
        {
            base.OnWindowVisibilityChanged(visibility);
            if (visibility == ViewStates.Visible)
                Init();
        }
    }
}

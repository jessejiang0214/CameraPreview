using System;
using System.Threading.Tasks;
using Android.Views;

namespace CameraPreview.Droid
{
    public class CameraAnalyzer
    {
        private readonly CameraController _cameraController;
        private DateTime _lastPreviewAnalysis = DateTime.UtcNow;
        private bool _wasScanned;
        private bool _cameraSetup;
        private readonly IDecoder _decoder;

        public CameraAnalyzer(CPSurfaceView surfaceView)
        {
            _cameraController = new CameraController(surfaceView);
            _decoder = CameraPreviewSettings.Instance.Decoder;
            _decoder.CanProcessImage = CanAnalyzeFrame;
            _decoder.FinishProcessImage = FinishProcessImage;
            _decoder.HandleExceptionFromProcessImage = HandleException;
        }

        public event EventHandler<IScanResult> ResultFound;

        public bool IsAnalyzing { get; private set; }

        public void PauseAnalysis()
        {
            IsAnalyzing = false;
        }

        public void ResumeAnalysis()
        {
            IsAnalyzing = true;
        }

        public void ShutdownCamera()
        {
            if (_cameraSetup)
            {
                IsAnalyzing = false;
                _cameraController.CloseCamera();
                _cameraSetup = false;
            }
        }

        public void SetupCamera(int width, int height)
        {
            if (!_cameraSetup)
            {
                _cameraController.OpenCamera(width, height);
                _cameraSetup = true;
            }
        }

        public void ConfigureTransform(int width, int height)
        {
            _cameraController.ConfigureTransform(width, height);
        }

        private bool CanAnalyzeFrame()
        {
            if (!IsAnalyzing)
                return false;
                
            var elapsedTimeMs = (DateTime.UtcNow - _lastPreviewAnalysis).TotalMilliseconds;
            if (elapsedTimeMs < CameraPreviewSettings.Instance.ScannerOptions.DelayBetweenAnalyzingFrames)
            {
                Logger.Log("Too soon between frames", LogLevel.Detail);
                return false;
            }

            // Delay a minimum between scans
            if (_wasScanned && elapsedTimeMs < CameraPreviewSettings.Instance.ScannerOptions.DelayBetweenContinuousScans)
            {
                Logger.Log("Too soon since last scan", LogLevel.Detail);
                return false;
            }

            _wasScanned = false;
            _lastPreviewAnalysis = DateTime.UtcNow;
            return true;
        }

        private bool FinishProcessImage(IScanResult result)
        {
            if (result != null)
            {
                _wasScanned = true;
                ResultFound?.Invoke(this, result);
            }
            return false;
        }

        private void HandleException(Exception ex)
        {
        }

    }
}

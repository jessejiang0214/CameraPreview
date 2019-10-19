using Android.Hardware.Camera2;

namespace CameraPreview.Droid
{
    public class CameraCaptureSessionCallback : CameraCaptureSession.StateCallback
    {
        private readonly CameraController _owner;

        public CameraCaptureSessionCallback(CameraController owner)
        {
            if (owner == null)
                throw new System.ArgumentNullException("owner");
            this._owner = owner;
        }

        public override void OnConfigureFailed(CameraCaptureSession session)
        {
            Logger.Log("Configure camera failed");
        }

        public override void OnConfigured(CameraCaptureSession session)
        {
            // The camera is already closed
            if (null == _owner.mCameraDevice)
            {
                return;
            }

            // When the session is ready, we start displaying the preview.
            _owner.CaptureSession = session;
            try
            {
                // Auto focus should be continuous for camera preview.
                _owner.PreviewRequestBuilder.Set(CaptureRequest.ControlAfMode, (int) ControlAFMode.ContinuousPicture);
                _owner.PreviewRequestBuilder.Set(CaptureRequest.ControlAeMode, (int) ControlAEMode.OnAutoFlash);
                // Flash is automatically enabled when necessary.
                _owner.SetAutoFlash(_owner.PreviewRequestBuilder);

                // Finally, we start displaying the camera preview.
                _owner.PreviewRequest = _owner.PreviewRequestBuilder.Build();
                _owner.CaptureSession.SetRepeatingRequest(_owner.PreviewRequest,
                    _owner.CaptureCallback, _owner.BackgroundHandler);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }
    }
}
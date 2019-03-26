using Android.Hardware.Camera2;
namespace CameraPreview.Droid
{
    public class CameraCaptureSessionCallback : CameraCaptureSession.StateCallback
    {
        private readonly CameraController owner;

        public CameraCaptureSessionCallback(CameraController owner)
        {
            if (owner == null)
                throw new System.ArgumentNullException("owner");
            this.owner = owner;
        }

        public override void OnConfigureFailed(CameraCaptureSession session)
        {
            Logger.Log("Configure camera failed");
        }

        public override void OnConfigured(CameraCaptureSession session)
        {
            // The camera is already closed
            if (null == owner.mCameraDevice)
            {
                return;
            }

            // When the session is ready, we start displaying the preview.
            owner.CaptureSession = session;
            try
            {
                // Auto focus should be continuous for camera preview.
                owner.PreviewRequestBuilder.Set(CaptureRequest.ControlAfMode, (int)ControlAFMode.ContinuousPicture);
                owner.PreviewRequestBuilder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.OnAutoFlash);
                // Flash is automatically enabled when necessary.
                //owner.SetAutoFlash(owner.PreviewRequestBuilder);

                // Finally, we start displaying the camera preview.
                owner.PreviewRequest = owner.PreviewRequestBuilder.Build();
                owner.CaptureSession.SetRepeatingRequest(owner.PreviewRequest,
                        owner.CaptureCallback, owner.BackgroundHandler);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }
    }
}

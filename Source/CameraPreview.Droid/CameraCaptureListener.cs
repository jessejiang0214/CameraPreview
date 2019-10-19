using Android.Hardware.Camera2;

namespace CameraPreview.Droid
{
    public class CameraCaptureListener : CameraCaptureSession.CaptureCallback
    {
        private readonly CameraController owner;

        public CameraCaptureListener(CameraController owner)
        {
            if (owner == null)
                throw new System.ArgumentNullException("owner");
            this.owner = owner;
        }

        public override void OnCaptureCompleted(CameraCaptureSession session, CaptureRequest request,
            TotalCaptureResult result)
        {
        }

        public override void OnCaptureProgressed(CameraCaptureSession session, CaptureRequest request,
            CaptureResult partialResult)
        {
        }
    }
}
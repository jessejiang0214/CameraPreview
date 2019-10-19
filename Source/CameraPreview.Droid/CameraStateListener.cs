using Android.Hardware.Camera2;

namespace CameraPreview.Droid
{
    public class CameraStateListener : CameraDevice.StateCallback
    {
        private readonly CameraController _owner;

        public CameraStateListener(CameraController owner)
        {
            if (owner == null)
                throw new System.ArgumentNullException("owner");
            this._owner = owner;
        }

        public override void OnOpened(CameraDevice cameraDevice)
        {
            // This method is called when the camera is opened.  We start camera preview here.
            _owner.mCameraOpenCloseLock.Release();
            _owner.mCameraDevice = cameraDevice;
            _owner.CreateCameraPreviewSession();
        }

        public override void OnDisconnected(CameraDevice cameraDevice)
        {
            _owner.mCameraOpenCloseLock.Release();
            cameraDevice.Close();
            _owner.mCameraDevice = null;
        }

        public override void OnError(CameraDevice cameraDevice, CameraError error)
        {
            _owner.mCameraOpenCloseLock.Release();
            cameraDevice.Close();
            _owner.mCameraDevice = null;
            if (_owner == null)
                return;
        }
    }
}
using System.Collections.Generic;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Java.Lang;
using Java.Util;
using Java.Util.Concurrent;
using Boolean = Java.Lang.Boolean;
using Math = Java.Lang.Math;
using Orientation = Android.Content.Res.Orientation;

namespace CameraPreview.Droid
{
    public class CameraController
    {
        private readonly Context _context;
        private readonly CPSurfaceView _surfaceView;
        private readonly CameraStateListener _cameraStateListener;
        private string _cameraId;
        public CameraDevice mCameraDevice;
        public Semaphore mCameraOpenCloseLock = new Semaphore(1);
        public CameraCaptureSession CaptureSession;
        public CaptureRequest.Builder PreviewRequestBuilder;
        public HandlerThread BackgroundThread;
        public Handler BackgroundHandler;
        public CaptureRequest PreviewRequest;
        public CameraCaptureListener CaptureCallback;
        private ImageReader _imageReader;
        private int _sensorOrientation;
        private IWindowManager _windowManager;

        // Max preview width that is guaranteed by Camera2 API
        private const int MAX_PREVIEW_WIDTH = 1920;

        // Max preview height that is guaranteed by Camera2 API
        private const int MAX_PREVIEW_HEIGHT = 1080;
        private Size _previewSize;
        private bool _flashSupported;

        public CameraController(CPSurfaceView surfaceView)
        {
            _context = Application.Context;
            _surfaceView = surfaceView;
            _cameraStateListener = new CameraStateListener(this);
            CaptureCallback = new CameraCaptureListener(this);
            _windowManager = _context
                .GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            StartBackgroundThread();
        }

        public int LastCameraDisplayOrientationDegree { get; private set; }

        public void OpenCamera(int width, int height)
        {
            if (ContextCompat.CheckSelfPermission(_context, Manifest.Permission.Camera) != Permission.Granted)
            {
                throw new RuntimeException("Don't have access to Camera!");
            }

            SetUpCameraOutputs(width, height);
            ConfigureTransform(width, height);

            var manager = (CameraManager) _context.GetSystemService(Context.CameraService);
            try
            {
                if (!mCameraOpenCloseLock.TryAcquire(2500, TimeUnit.Milliseconds))
                {
                    throw new RuntimeException("Time out waiting to lock camera opening.");
                }

                manager.OpenCamera(_cameraId, _cameraStateListener, BackgroundHandler);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
            catch (InterruptedException e)
            {
                throw new RuntimeException("Interrupted while trying to lock camera opening.", e);
            }
        }

        private void SetUpCameraOutputs(int width, int height)
        {
            var manager = (CameraManager) _context.GetSystemService(Context.CameraService);
            try
            {
                for (var i = 0; i < manager.GetCameraIdList().Length; i++)
                {
                    var cameraId = manager.GetCameraIdList()[i];
                    var characteristics = manager.GetCameraCharacteristics(cameraId);

                    // We don't use a front facing camera in this sample.
                    var facing = (Integer) characteristics.Get(CameraCharacteristics.LensFacing);
                    if (facing != null && facing == (Integer.ValueOf((int) LensFacing.Front)))
                    {
                        continue;
                    }

                    var map = (StreamConfigurationMap) characteristics.Get(CameraCharacteristics
                        .ScalerStreamConfigurationMap);
                    if (map == null)
                    {
                        continue;
                    }

                    // For still image captures, we use the largest available size.
                    var largest = (Size) Collections.Max(Arrays.AsList(map.GetOutputSizes((int) ImageFormatType.Jpeg)),
                        new CompareSizesByArea());

                    _imageReader = ImageReader.NewInstance(largest.Width, largest.Height,
                        ImageFormatType.Jpeg, /*maxImages*/2);

                    // Find out if we need to swap dimension to get the preview size relative to sensor
                    // coordinate.
                    var displayRotation = _windowManager.DefaultDisplay.Rotation;
                    //noinspection ConstantConditions
                    _sensorOrientation = (int) characteristics.Get(CameraCharacteristics.SensorOrientation);
                    var swappedDimensions = false;
                    switch (displayRotation)
                    {
                        case SurfaceOrientation.Rotation0:
                        case SurfaceOrientation.Rotation180:
                            if (_sensorOrientation == 90 || _sensorOrientation == 270)
                            {
                                swappedDimensions = true;
                            }

                            break;
                        case SurfaceOrientation.Rotation90:
                        case SurfaceOrientation.Rotation270:
                            if (_sensorOrientation == 0 || _sensorOrientation == 180)
                            {
                                swappedDimensions = true;
                            }

                            break;
                        default:
                            //Log.Error(TAG, "Display rotation is invalid: " + displayRotation);
                            break;
                    }

                    var displaySize = new Point();
                    _windowManager.DefaultDisplay.GetSize(displaySize);
                    var rotatedPreviewWidth = width;
                    var rotatedPreviewHeight = height;
                    var maxPreviewWidth = displaySize.X;
                    var maxPreviewHeight = displaySize.Y;

                    if (swappedDimensions)
                    {
                        rotatedPreviewWidth = height;
                        rotatedPreviewHeight = width;
                        maxPreviewWidth = displaySize.Y;
                        maxPreviewHeight = displaySize.X;
                    }

                    if (maxPreviewWidth > MAX_PREVIEW_WIDTH)
                    {
                        maxPreviewWidth = MAX_PREVIEW_WIDTH;
                    }

                    if (maxPreviewHeight > MAX_PREVIEW_HEIGHT)
                    {
                        maxPreviewHeight = MAX_PREVIEW_HEIGHT;
                    }

                    // Danger, W.R.! Attempting to use too large a preview size could  exceed the camera
                    // bus' bandwidth limitation, resulting in gorgeous previews but the storage of
                    // garbage capture data.
                    _previewSize = ChooseOptimalSize(map.GetOutputSizes(Class.FromType(typeof(SurfaceTexture))),
                        rotatedPreviewWidth, rotatedPreviewHeight, maxPreviewWidth,
                        maxPreviewHeight, largest);

                    // We fit the aspect ratio of TextureView to the size of preview we picked.
                    var orientation = _context.Resources.Configuration.Orientation;
                    if (orientation == Orientation.Landscape)
                    {
                        _surfaceView.SetAspectRatio(_previewSize.Width, _previewSize.Height);
                    }
                    else
                    {
                        _surfaceView.SetAspectRatio(_previewSize.Height, _previewSize.Width);
                    }

                    // Check if the flash is supported.
                    var available = (Boolean) characteristics.Get(CameraCharacteristics.FlashInfoAvailable);
                    if (available == null)
                    {
                        _flashSupported = false;
                    }
                    else
                    {
                        _flashSupported = (bool) available;
                    }

                    _cameraId = cameraId;
                    return;
                }
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
            catch (NullPointerException e)
            {
                // Currently an NPE is thrown when the Camera2API is used but not supported on the
                // device this code runs.
                e.PrintStackTrace();
                throw new RuntimeException("Don't have access to Camera!");
            }
        }

        private static Size ChooseOptimalSize(Size[] choices,
            int textureViewWidth,
            int textureViewHeight,
            int maxWidth,
            int maxHeight,
            Size aspectRatio)
        {
            // Collect the supported resolutions that are at least as big as the preview Surface
            var bigEnough = new List<Size>();
            // Collect the supported resolutions that are smaller than the preview Surface
            var notBigEnough = new List<Size>();
            var w = aspectRatio.Width;
            var h = aspectRatio.Height;

            foreach (var option in choices)
            {
                if ((option.Width <= maxWidth) && (option.Height <= maxHeight) &&
                    option.Height == option.Width * h / w)
                {
                    if (option.Width >= textureViewWidth &&
                        option.Height >= textureViewHeight)
                    {
                        bigEnough.Add(option);
                    }
                    else
                    {
                        notBigEnough.Add(option);
                    }
                }
            }

            // Pick the smallest of those big enough. If there is no one big enough, pick the
            // largest of those not big enough.
            if (bigEnough.Count > 0)
            {
                return (Size) Collections.Min(bigEnough, new CompareSizesByArea());
            }
            else if (notBigEnough.Count > 0)
            {
                return (Size) Collections.Max(notBigEnough, new CompareSizesByArea());
            }
            else
            {
                //Log.Error(TAG, "Couldn't find any suitable preview size");
                return choices[0];
            }
        }


        public void ConfigureTransform(int viewWidth, int viewHeight)
        {
            //var activity = (Activity)_context;
            if (null == _surfaceView || null == _previewSize || null == _context)
            {
                return;
            }

            var windowManager = _context
                .GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            ;
            var rotation = (int) windowManager.DefaultDisplay.Rotation;
            var matrix = new Matrix();
            var viewRect = new RectF(0, 0, viewWidth, viewHeight);
            var bufferRect = new RectF(0, 0, _previewSize.Height, _previewSize.Width);
            var centerX = viewRect.CenterX();
            var centerY = viewRect.CenterY();
            if ((int) SurfaceOrientation.Rotation90 == rotation || (int) SurfaceOrientation.Rotation270 == rotation)
            {
                bufferRect.Offset(centerX - bufferRect.CenterX(), centerY - bufferRect.CenterY());
                matrix.SetRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.Fill);
                var scale = Math.Max((float) viewHeight / _previewSize.Height, (float) viewWidth / _previewSize.Width);
                matrix.PostScale(scale, scale, centerX, centerY);
                matrix.PostRotate(90 * (rotation - 2), centerX, centerY);
            }
            else if ((int) SurfaceOrientation.Rotation180 == rotation)
            {
                matrix.PostRotate(180, centerX, centerY);
            }

            _surfaceView.SetTransform(matrix);
        }

        public void CloseCamera()
        {
            try
            {
                mCameraOpenCloseLock.Acquire();
                if (null != CaptureSession)
                {
                    CaptureSession.Close();
                    CaptureSession = null;
                }

                if (null != mCameraDevice)
                {
                    mCameraDevice.Close();
                    mCameraDevice = null;
                }

                if (null != _imageReader)
                {
                    _imageReader.Close();
                    _imageReader = null;
                }

                StopBackgroundThread();
            }
            catch (InterruptedException e)
            {
                throw new RuntimeException("Interrupted while trying to lock camera closing.", e);
            }
            finally
            {
                mCameraOpenCloseLock.Release();
            }
        }

        private void StartBackgroundThread()
        {
            BackgroundThread = new HandlerThread("CameraPreviewWorkThread");
            BackgroundThread.Start();
            Logger.Log("Camera thread is start");
            BackgroundHandler = new Handler(BackgroundThread.Looper);
        }

        // Stops the background thread and its {@link Handler}.
        private void StopBackgroundThread()
        {
            BackgroundThread.QuitSafely();
            try
            {
                BackgroundThread.Join();
                BackgroundThread = null;
                BackgroundHandler = null;
            }
            catch (InterruptedException e)
            {
                e.PrintStackTrace();
            }
        }

        private class ImageDecoder : Java.Lang.Object, IRunnable
        {
            private readonly IDecoder _decoder;
            private readonly Handler _backgroundHandler;
            private readonly CPSurfaceView _surfaceView;

            public ImageDecoder(Handler backgroundHandler, CPSurfaceView surfaceView)
            {
                _decoder = CameraPreviewSettings.Instance.Decoder;
                _backgroundHandler = backgroundHandler;
                _surfaceView = surfaceView;
            }

            public void Run()
            {
                Bitmap bitmap = null;
                try
                {
                    if (_decoder.CanProcessImage == null || !_decoder.CanProcessImage())
                    {
                        _backgroundHandler.Post(new ImageDecoder(_backgroundHandler, _surfaceView));
                        return;
                    }

                    bitmap = _surfaceView.GetBitmap(_decoder.ImageSizeX, _decoder.ImageSizeY);
                    var result = _decoder.Decode(bitmap);
                    bitmap.Recycle();
                    if (_decoder.FinishProcessImage(result))
                        return;
                }
                catch (System.Exception ex)
                {
                    bitmap?.Recycle();
                    _decoder.HandleExceptionFromProcessImage?.Invoke(ex);
                }

                _backgroundHandler.Post(new ImageDecoder(_backgroundHandler, _surfaceView));
            }
        }

        public void CreateCameraPreviewSession()
        {
            try
            {
                var texture = _surfaceView.SurfaceTexture;
                if (texture == null)
                {
                    throw new IllegalStateException("texture is null");
                }

                // We configure the size of default buffer to be the size of camera preview we want.
                texture.SetDefaultBufferSize(_previewSize.Width, _previewSize.Height);

                // This is the output Surface we need to start preview.
                var surface = new Surface(texture);

                // We set up a CaptureRequest.Builder with the output Surface.
                PreviewRequestBuilder = mCameraDevice.CreateCaptureRequest(CameraTemplate.Preview);
                PreviewRequestBuilder.AddTarget(surface);

                // Here, we create a CameraCaptureSession for camera preview.
                var surfaces = new List<Surface> {surface};
                mCameraDevice.CreateCaptureSession(surfaces, new CameraCaptureSessionCallback(this), null);

                BackgroundHandler.Post(new ImageDecoder(BackgroundHandler, _surfaceView));

            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }

        public void SetAutoFlash(CaptureRequest.Builder requestBuilder)
        {
            if (_flashSupported)
            {
                requestBuilder.Set(CaptureRequest.ControlAeMode, (int) ControlAEMode.OnAutoFlash);
            }
        }
    }
}
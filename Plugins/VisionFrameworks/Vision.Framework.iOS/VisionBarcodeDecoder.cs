using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CameraPreview;
using CameraPreview.iOS;
using CoreGraphics;
using CoreVideo;
using Foundation;
using Vision.Framework.Xamarin.Forms;

namespace Vision.Framework.iOS
{
    public class VisionBarcodeDecoder : DefaultDecoderBase
    {
        VNDetectBarcodesRequest barcodesRequest;
        TaskCompletionSource<IScanResult> _barcodeResult = null;
        nint _width = 0;
        nint _height = 0;
        public VisionBarcodeDecoder() : base()
        {
            barcodesRequest = new VNDetectBarcodesRequest(HandleBarCodes);
        }

        public override IScanResult Decode(CVPixelBuffer pixelBuffer)
        {
            var decoder = PerformanceCounter.Start();
            _width = pixelBuffer.Width;
            _height = pixelBuffer.Height;
            _barcodeResult = new TaskCompletionSource<IScanResult>();
            var handler = new VNImageRequestHandler(pixelBuffer, new VNImageOptions());
            handler.Perform(new VNRequest[] { barcodesRequest }, out NSError error);
            _barcodeResult.Task.Wait();
            PerformanceCounter.Stop(decoder, "Vision framework Decoder take {0} ms.");
            return _barcodeResult.Task.Result;
        }

        void HandleBarCodes(VNRequest request, NSError error)
        {
            if (error != null)
            {
                _barcodeResult.SetException(new NSErrorException(error));
                return;
            }
            var observations = request.GetResults<VNBarcodeObservation>();
            var result = new VisionBarCodeResult();
            result.Success = true;
            result.Timestamp = DateTime.Now.Ticks;
            result.ImageWidth = (int)_width;
            result.ImageHeight = (int)_height;

            foreach (var o in observations)
            {
                if (result.Results == null)
                {
                    result.Results = new List<BarCodeResult>();
                }
                var res = new BarCodeResult
                {
                    Text = o.PayloadStringValue,
                    X = o.BoundingBox.X * _width,
                    Y = o.BoundingBox.Y * _height,
                    Width = o.BoundingBox.Width * _width,
                    Height = o.BoundingBox.Height * _height,
                };
                result.Results.Add(res);
            }
            _barcodeResult.SetResult(result);
        }

    }
}

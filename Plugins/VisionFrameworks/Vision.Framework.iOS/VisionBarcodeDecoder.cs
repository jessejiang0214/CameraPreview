using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CameraPreview;
using CameraPreview.iOS;
using CoreVideo;
using Foundation;
using Vision.Framework.Xamarin.Forms;

namespace Vision.Framework.iOS
{
    public class VisionBarcodeDecoder : DefaultDecoderBase
    {
        VNDetectBarcodesRequest barcodesRequest;
        TaskCompletionSource<IScanResult> _barcodeResult = null;
        public VisionBarcodeDecoder() : base()
        {
            barcodesRequest = new VNDetectBarcodesRequest(HandleBarCodes);
        }

        public override IScanResult Decode(CVPixelBuffer pixelBuffer)
        {
            var decoder = PerformanceCounter.Start();
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
            foreach (var o in observations)
            {
                if (result.Results == null)
                {
                    result.Results = new List<BarCodeResult>();
                }
                var res = new BarCodeResult
                {
                    Text = o.PayloadStringValue,
                    X = o.TopLeft.X,
                    Y = o.TopLeft.Y,
                    Width = o.TopRight.X - o.TopLeft.X,
                    Height = o.BottomLeft.Y - o.TopLeft.Y,
                };
                result.Results.Add(res);
                Logger.Log($"Found bar code {res.Text} {res.X} {res.Y} {res.Width} {res.Height}");
            }
            _barcodeResult.SetResult(result);
        }

    }
}

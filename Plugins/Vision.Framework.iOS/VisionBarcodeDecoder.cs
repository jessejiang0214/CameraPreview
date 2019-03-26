using System;
using System.Linq;
using System.Threading.Tasks;
using CameraPreview;
using CameraPreview.iOS;
using CoreFoundation;
using CoreVideo;
using Foundation;
using ZXing.Net.Xamarin.Forms;

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
            Logger.Log("Decode start");
            _barcodeResult = new TaskCompletionSource<IScanResult>();
            var handler = new VNImageRequestHandler(pixelBuffer, new VNImageOptions());
            handler.Perform(new VNRequest[] { barcodesRequest }, out NSError error);
            _barcodeResult.Task.Wait();
            PerformanceCounter.Stop(decoder, "Vision framework Decoder take {0} ms.");
            return _barcodeResult.Task.Result;
        }

        void HandleBarCodes(VNRequest request, NSError error)
        {
            Logger.Log("Decode get response");
            if (error != null)
            {
                _barcodeResult.SetException(new NSErrorException(error));
                return;
            }
            var observations = request.GetResults<VNBarcodeObservation>();
            var result = new ZXingResult();
            result.Success = true;
            result.Timestamp = DateTime.Now.Ticks;
            if (observations.Any())
                result.Text = observations.First().PayloadStringValue;
            _barcodeResult.SetResult(result);
        }

    }
}

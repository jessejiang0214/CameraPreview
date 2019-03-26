using System;
using CameraPreview;
using CameraPreview.iOS;
using CoreVideo;
using ZXing.Net.Xamarin.Forms;

namespace ZXing.Net.iOS
{
    public class ZXingDecoder : DefaultDecoderBase
    {
        BarcodeReader _reader;
        public ZXingDecoder() : base()
        {
            _reader = new BarcodeReader();
        }

        public override IScanResult Decode(CVPixelBuffer pixelBuffer)
        {
            var decoder = PerformanceCounter.Start();
            unsafe
            {
                var rawData = (byte*)pixelBuffer.BaseAddress.ToPointer();
                int rawDatalen = (int)(pixelBuffer.Height * pixelBuffer.Width * 4);
                int width = (int)pixelBuffer.Width;
                int height = (int)pixelBuffer.Height;
                var luminanceSource = new CVPixelBufferBGRA32LuminanceSource(rawData, rawDatalen, width, height);
                var res = _reader.Decode(luminanceSource);
                var result = new ZXingResult(res);
                PerformanceCounter.Stop(decoder, "ZXing Decoder take {0} ms.");
                return result;
            }
        }

        public override void ScanningOptionsUpdate(ScanningOptionsBase options)
        {
            if (options is ZXingOptions zOpt)
                _reader = zOpt.BuildBarcodeReader();
        }
    }
}

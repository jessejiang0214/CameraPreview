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
                var rawData = (byte*) pixelBuffer.BaseAddress.ToPointer();
                int rawDatalen = (int) (pixelBuffer.Height * pixelBuffer.Width * 4);
                int width = (int) pixelBuffer.Width;
                int height = (int) pixelBuffer.Height;
                var luminanceSource = new CVPixelBufferBGRA32LuminanceSource(rawData, rawDatalen, width, height);
                var res = _reader.Decode(luminanceSource);
                var result = new ZXingResult();
                if (res == null)
                {
                    result.Success = false;
                    result.Timestamp = DateTime.Now.Ticks;
                }
                else
                {
                    result.Success = true;
                    result.Timestamp = res.Timestamp;
                    result.Text = res.Text;
                }

                PerformanceCounter.Stop(decoder, "ZXing Decoder take {0} ms.");
                return result;
            }
        }

        public override void ScanningOptionsUpdate(ScanningOptionsBase options)
        {
            if (options is ZXingOptions zOpt)
            {
                if (zOpt.TryHarder.HasValue)
                    _reader.Options.TryHarder = zOpt.TryHarder.Value;
                if (zOpt.PureBarcode.HasValue)
                    _reader.Options.PureBarcode = zOpt.PureBarcode.Value;
                if (zOpt.AutoRotate.HasValue)
                    _reader.AutoRotate = zOpt.AutoRotate.Value;
                if (zOpt.UseCode39ExtendedMode.HasValue)
                    _reader.Options.UseCode39ExtendedMode = zOpt.UseCode39ExtendedMode.Value;
                if (!string.IsNullOrEmpty(zOpt.CharacterSet))
                    _reader.Options.CharacterSet = zOpt.CharacterSet;
                if (zOpt.TryInverted.HasValue)
                    _reader.TryInverted = zOpt.TryInverted.Value;
                if (zOpt.AssumeGS1.HasValue)
                    _reader.Options.AssumeGS1 = zOpt.AssumeGS1.Value;
            }
        }
    }
}
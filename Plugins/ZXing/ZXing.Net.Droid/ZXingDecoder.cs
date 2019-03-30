using System;
using Android.Graphics;
using CameraPreview;
using CameraPreview.Droid;
using ZXing.Net.Xamarin.Forms;
using ZXing;

namespace ZXing.Net.Droid
{
    public class ZXingDecoder : DefaultDecoderBase
    {
        BarcodeReader _reader;
        public ZXingDecoder()
        {
            _reader = new BarcodeReader();
        }

        public override IScanResult Decode(Bitmap image)
        {
            var decoder = PerformanceCounter.Start();
            var res = _reader.Decode(image);
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

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
            var bitmapSource = new BitmapLuminanceSource(image);
            var res = _reader.Decode(bitmapSource);
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
                _reader = zOpt.BuildBarcodeReader();
        }
    }
}

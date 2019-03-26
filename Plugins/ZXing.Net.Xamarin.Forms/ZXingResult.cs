using System;
using CameraPreview;

namespace ZXing.Net.Xamarin.Forms
{
    public class ZXingResult : IScanResult
    {
        public ZXingResult(Result zxingResult)
        {
            if (zxingResult == null)
            {
                Timestamp = DateTime.Now.Ticks;
                Success = false;
            }
            else
            {
                Timestamp = zxingResult.Timestamp;
                Text = zxingResult.Text;
                Success = true;
            }


        }

        public ZXingResult()
        {

        }
        public long Timestamp { get; set; }

        public bool Success { get; set; }

        public string Text { get; set; }
    }
}

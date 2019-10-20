using CameraPreview;

namespace ZXing.Net.Xamarin.Forms
{
    public class ZXingResult : IScanResult
    {
        public long Timestamp { get; set; }

        public bool Success { get; set; }

        public string Text { get; set; }
    }
}
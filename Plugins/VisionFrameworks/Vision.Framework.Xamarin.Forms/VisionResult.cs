using System;
using System.Collections.Generic;
using CameraPreview;

namespace Vision.Framework.Xamarin.Forms
{
    public class VisionBarCodeResult : IScanResult
    {
        public long Timestamp { get; set; }

        public bool Success { get; set; }

        public IList<BarCodeResult> Results { get; set; }
    }

    public struct BarCodeResult
    {
        public string Text;
        public double X;
        public double Y;
        public double Width;
        public double Height;
    }

}


using System;
using System.Collections.Generic;
using CameraPreview;

namespace ZXing.Net.Xamarin.Forms
{
    public class ZXingOptions : ScanningOptionsBase
    {
        public ZXingOptions() : base()
        {
        }

        public bool? TryHarder { get; set; }
        public bool? PureBarcode { get; set; }
        public bool? AutoRotate { get; set; }
        public bool? UseCode39ExtendedMode { get; set; }
        public string CharacterSet { get; set; }
        public bool? TryInverted { get; set; }
        public bool? AssumeGS1 { get; set; }

    }
}

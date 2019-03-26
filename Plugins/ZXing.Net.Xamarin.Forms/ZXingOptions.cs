using System;
using System.Collections.Generic;
using CameraPreview;

namespace ZXing.Net.Xamarin.Forms
{
    public class ZXingOptions : ScanningOptionsBase
    {
        public ZXingOptions() : base()
        {
            this.PossibleFormats = new List<BarcodeFormat>(); 
        }


        public List<BarcodeFormat> PossibleFormats { get; set; }
        public bool? TryHarder { get; set; }
        public bool? PureBarcode { get; set; }
        public bool? AutoRotate { get; set; }
        public bool? UseCode39ExtendedMode { get; set; }
        public string CharacterSet { get; set; }
        public bool? TryInverted { get; set; }
        public bool? AssumeGS1 { get; set; }


        public BarcodeReader BuildBarcodeReader()
        {
            var reader = new BarcodeReader();
            if (this.TryHarder.HasValue)
                reader.Options.TryHarder = this.TryHarder.Value;
            if (this.PureBarcode.HasValue)
                reader.Options.PureBarcode = this.PureBarcode.Value;
            if (this.AutoRotate.HasValue)
                reader.AutoRotate = this.AutoRotate.Value;
            if (this.UseCode39ExtendedMode.HasValue)
                reader.Options.UseCode39ExtendedMode = this.UseCode39ExtendedMode.Value;
            if (!string.IsNullOrEmpty(this.CharacterSet))
                reader.Options.CharacterSet = this.CharacterSet;
            if (this.TryInverted.HasValue)
                reader.TryInverted = this.TryInverted.Value;
            if (this.AssumeGS1.HasValue)
                reader.Options.AssumeGS1 = this.AssumeGS1.Value;

            if (this.PossibleFormats != null && this.PossibleFormats.Count > 0)
            {
                reader.Options.PossibleFormats = new List<BarcodeFormat>();

                foreach (var pf in this.PossibleFormats)
                    reader.Options.PossibleFormats.Add(pf);
            }

            return reader;
        }
    }
}

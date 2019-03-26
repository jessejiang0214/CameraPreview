using System;
namespace CameraPreview.Droid
{
    public class CameraPreviewSettings
    {
        private static CameraPreviewSettings instance;
        private ScanningOptionsBase _scanningOptions = ScanningOptionsBase.Default;
        private CameraPreviewSettings() { }

        public static CameraPreviewSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CameraPreviewSettings();
                }
                return instance;
            }
        }

        public IDecoder Decoder { get; set; }

        public ScanningOptionsBase ScannerOptions => _scanningOptions;

        public virtual void SetScannerOptions(ScanningOptionsBase value)
        {
            if (value == null)
                return;
            _scanningOptions = value;
            Decoder.ScanningOptionsUpdate(value);
        }

        public virtual void Init(IDecoder decoder)
        {
            if (decoder == null)
                Decoder = new DefaultDecoderBase();
            else
                Decoder = decoder;
        }
    }
}

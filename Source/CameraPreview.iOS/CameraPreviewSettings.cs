using System;
namespace CameraPreview.iOS
{
    public class CameraPreviewSettings
    {
        private static CameraPreviewSettings instance;
        private ScanningOptionsBase _scanningOptions = ScanningOptionsBase.Default;
        protected CameraPreviewSettings() { }

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

        public void Init(IDecoder decoder)
        {
            if (decoder == null)
                Decoder = new DefaultDecoderBase();
            else
                Decoder = decoder;
        }
    }
}

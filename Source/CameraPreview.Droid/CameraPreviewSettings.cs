namespace CameraPreview.Droid
{
    public class CameraPreviewSettings
    {
        private static CameraPreviewSettings _instance;

        private CameraPreviewSettings()
        {
        }

        public static CameraPreviewSettings Instance => _instance ?? (_instance = new CameraPreviewSettings());

        public IDecoder Decoder { get; set; }

        public ScanningOptionsBase ScannerOptions { get; private set; } = ScanningOptionsBase.Default;

        public virtual void SetScannerOptions(ScanningOptionsBase value)
        {
            if (value == null)
                return;
            ScannerOptions = value;
            Decoder.ScanningOptionsUpdate(value);
        }

        public virtual void Init(IDecoder decoder)
        {
            Decoder = decoder ?? new DefaultDecoderBase();
        }
    }
}
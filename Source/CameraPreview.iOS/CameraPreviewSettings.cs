namespace CameraPreview.iOS
{
    public class CameraPreviewSettings
    {
        private static CameraPreviewSettings _instance;

        protected CameraPreviewSettings()
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

        public void Init(IDecoder decoder)
        {
            Decoder = decoder ?? new DefaultDecoderBase();
        }
    }
}
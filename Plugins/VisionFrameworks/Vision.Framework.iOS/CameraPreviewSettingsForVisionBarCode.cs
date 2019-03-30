using System;
using CameraPreview.iOS;

namespace Vision.Framework.iOS
{
    public class CameraPreviewSettingsForVisionBarCode
    {
        public static void Init()
        {
            CameraPreviewSettings.Instance.Init(new VisionBarcodeDecoder());
        }
    }
}

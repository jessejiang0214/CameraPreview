using System;
using CameraPreview.iOS;

namespace ZXing.Net.iOS
{
    public class CameraPreviewSettingsForZXing
    {
        public static void Init()
        {
            CameraPreviewSettings.Instance.Init(new ZXingDecoder());
        }
    }
}

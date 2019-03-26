using System;
using CameraPreview.Droid;

namespace ZXing.Net.Droid
{
    public class CameraPreviewSettingsForZXing
    {
        public static void Init()
        {
            CameraPreviewSettings.Instance.Init(new ZXingDecoder());
        }
    }
}

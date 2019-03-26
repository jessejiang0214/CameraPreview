using System;
namespace CameraPreview
{
    public class DefaultScanResult : IScanResult
    {
        public long Timestamp => DateTime.Now.Ticks;

        public bool Success => true;
    }
}

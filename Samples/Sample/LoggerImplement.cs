using System;
using CameraPreview;

namespace Sample
{
    public class LoggerImplement : ILogger
    {
        public void Log(string message, LogLevel level)
        {
            if (level == LogLevel.Detail)
                return;
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}

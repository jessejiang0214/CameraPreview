using System;
using CameraPreview;

namespace Sample
{
    public class LoggerImplement : ILogger
    {
        public void Log(string message, LogLevel leve)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}

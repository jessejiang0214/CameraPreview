using System;
namespace CameraPreview
{
    public interface ILogger
    {
        void Log(string message, LogLevel leve);
    }

    public enum LogLevel
    {
        Normal,
        Detail,
        Warring,
        Error
    }
}

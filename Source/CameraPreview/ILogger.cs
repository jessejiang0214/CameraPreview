namespace CameraPreview
{
    public interface ILogger
    {
        void Log(string message, LogLevel level);
    }

    public enum LogLevel
    {
        Normal,
        Detail,
        Warring,
        Error
    }
}
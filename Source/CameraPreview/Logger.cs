using System;
namespace CameraPreview
{
    public class Logger
    {
        private static Logger instance;

        private Logger() { }

        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Logger();
                }
                return instance;
            }
        }

        public static void Log(string message, LogLevel level = LogLevel.Normal)
        {
            if (Instance.LoggerObj == null)
            {
                if (level != LogLevel.Detail)
                    System.Diagnostics.Debug.WriteLine(message);
                return;
            }
            instance.LoggerObj.Log(message, level);
        }

        public ILogger LoggerObj { get; set; }
    }
}

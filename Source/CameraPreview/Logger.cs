namespace CameraPreview
{
    public class Logger
    {
        private static Logger _instance;

        private Logger()
        {
        }

        public static Logger Instance => _instance ?? (_instance = new Logger());

        public static void Log(string message, LogLevel level = LogLevel.Normal)
        {
            if (Instance.LoggerObj == null)
            {
                if (level != LogLevel.Detail)
                    System.Diagnostics.Debug.WriteLine(message);
                return;
            }

            _instance.LoggerObj.Log(message, level);
        }

        public ILogger LoggerObj { get; set; }
    }
}
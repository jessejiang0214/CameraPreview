namespace CameraPreview
{
    public interface IScanResult
    {
        long Timestamp { get; }

        bool Success { get; }
    }
}
namespace FolderSync.Core.Logger;

public class SyncLogger : ISyncLogger
{
    private FileInfo _logFilePath;

    public SyncLogger(string logFilePath)
    {
        _logFilePath = new FileInfo(logFilePath);
        Log("INFO", $"Initialized logger with log file path: {logFilePath}");
    }

    public void LogInfo(string message) => Log("INFO", message);
    public void LogWarning(string message) => Log("WARNING", message);
    public void LogError(string message, Exception? ex = null)
    {
        Log("ERROR", message);
        if (ex != null)
        {
            Log("ERROR", ex.ToString());
        }
    }

    private void Log(string level, string message)
    {
        var formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        Console.WriteLine(formattedMessage);

        File.AppendAllText(_logFilePath.FullName, $"{formattedMessage}{Environment.NewLine}");
    }
}
namespace FolderSync.Core.Logger;

public interface ISyncLogger
{
    void LogInfo(string message);
    void LogDebug(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? ex = null);
}
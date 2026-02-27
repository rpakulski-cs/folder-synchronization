namespace FolderSync.Core.Logger;

public interface ISyncLogger
{
    public void LogInfo(string message);
    public void LogWarning(string message);
    public void LogError(string message, Exception? ex = null);
}
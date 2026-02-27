namespace FolderSync.Core.FileUtils;
    
public interface IFileSystem
{
    bool DirectoryExists(string path);
    bool FileExists(string path);
    IEnumerable<string> GetDirectories(string path);
    IEnumerable<string> GetFiles(string path);
    FileDetails GetFileDetails(string path);
    Stream FileOpenRead(string path);
    void CreateDirectory(string path);
    void DeleteDirectoryRecursively(string path);
    void DeleteFile(string path);
    void CopyFileAndOverwrite(string sourcePath, string destinationPath);
}

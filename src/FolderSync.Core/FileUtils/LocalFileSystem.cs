namespace FolderSync.Core.FileUtils;

public class LocalFileSystem : IFileSystem
{
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public bool FileExists(string path) => File.Exists(path);

    public IEnumerable<string> GetDirectories(string path) => Directory.EnumerateDirectories(path);

    public IEnumerable<string> GetFiles(string path) => Directory.EnumerateFiles(path);

    public FileDetails GetFileDetails(string path)
    {
        var fileInfo = new FileInfo(path);
        return new FileDetails(fileInfo.Length, fileInfo.LastWriteTimeUtc);
    }

    public Stream FileOpenRead(string path) => File.OpenRead(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public void DeleteDirectoryRecursively(string path) => Directory.Delete(path, true);

    public void DeleteFile(string path) =>File.Delete(path);

    public void CopyFileAndOverwrite(string sourcePath, string destinationPath)
        => File.Copy(sourcePath, destinationPath, true);

}   
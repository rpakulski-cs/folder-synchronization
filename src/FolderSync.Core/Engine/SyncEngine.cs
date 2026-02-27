using FolderSync.Core.FileUtils;
using FolderSync.Core.Logger;

namespace FolderSync.Core
{
    public class SyncEngine
    {
        private readonly ISyncLogger _logger;
        private readonly IFileSystem _fileSystem;

        public SyncEngine(ISyncLogger logger, IFileSystem fileSystem)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public void Synchronize(string sourcePath, string destinationPath)
        {
            if (string.IsNullOrEmpty(sourcePath))
                throw new ArgumentException("Value cannot be null or empty.", nameof(sourcePath));
            if (string.IsNullOrEmpty(destinationPath))
                throw new ArgumentException("Value cannot be null or empty.", nameof(destinationPath));

            _logger.LogInfo($"Starting synchronization from '{sourcePath}' to '{destinationPath}'.");

            SynchronizeStack(sourcePath, destinationPath);

            _logger.LogInfo("Synchronization completed.");
        }

        private void SynchronizeStack(string sourcePath, string destinationPath)
        {
            var dirsToProcess = new Stack<string>();
            dirsToProcess.Push(sourcePath);

            while (dirsToProcess.Count > 0)
            {
                string currentSourceDir = dirsToProcess.Pop();
                string relativePath = Path.GetRelativePath(sourcePath, currentSourceDir);
                string currentReplicaDir = relativePath == "."
                    ? destinationPath
                    : Path.Combine(destinationPath, relativePath);

                if (!_fileSystem.DirectoryExists(currentReplicaDir))
                {
                    _fileSystem.CreateDirectory(currentReplicaDir);
                    _logger.LogInfo($"Created directory '{currentReplicaDir}' (NEW DIRECTORY).");
                }

                var filesInSourceDir = _fileSystem.GetFiles(currentSourceDir);
                foreach (var sourceFilePath in filesInSourceDir)
                {
                    var sourceDetails = _fileSystem.GetFileDetails(sourceFilePath);
                    var replicaFilePath = Path.Combine(currentReplicaDir, Path.GetFileName(sourceFilePath));
                   
                    if (!_fileSystem.FileExists(replicaFilePath))
                    {
                        _fileSystem.CopyFileAndOverwrite(sourceFilePath, replicaFilePath);
                        _logger.LogInfo($"Copied file '{sourceFilePath}' (NEW FILE) to '{replicaFilePath}'.");
                        continue;
                    }

                    var replicaDetails = _fileSystem.GetFileDetails(replicaFilePath);

                    if (sourceDetails != replicaDetails)
                    {
                        _fileSystem.CopyFileAndOverwrite(sourceFilePath, replicaFilePath);
                        _logger.LogInfo($"Copied file '{sourceFilePath}' (MODIFIED) to '{replicaFilePath}'.");
                    }
                    else if (!CompareMD5(sourceFilePath, replicaFilePath))
                    {
                        _fileSystem.CopyFileAndOverwrite(sourceFilePath, replicaFilePath);
                        _logger.LogInfo($"Copied file '{sourceFilePath}' (MD5 CHANGED) to '{replicaFilePath}'.");
                    }
                    else
                    {
                        _logger.LogInfo($"File '{sourceFilePath}' is up to date. No action needed.");
                    }
                }

                try
                {
                    var subDirs = _fileSystem.GetDirectories(currentSourceDir);
                    foreach (var subDir in subDirs)
                        dirsToProcess.Push(subDir);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to get directories from '{currentSourceDir}'. Skipping this directory.", ex);
                    continue;
                }
            }

            RemoveRedundantFilesAndDirsFromReplicaDir(sourcePath, destinationPath);
        }

        private bool CompareMD5(string filePath1, string filePath2)
        {
            using var stream1 = _fileSystem.FileOpenRead(filePath1);
            using var stream2 = _fileSystem.FileOpenRead(filePath2);

            using var md5 = System.Security.Cryptography.MD5.Create();
            var hash1 = md5.ComputeHash(stream1);
            var hash2 = md5.ComputeHash(stream2);

            return hash1.SequenceEqual(hash2);
        }

        private void RemoveRedundantFilesAndDirsFromReplicaDir(string sourcePath, string replicaPath)
        {
            var dirsToProcess = new Stack<string>();
            dirsToProcess.Push(replicaPath);

            while (dirsToProcess.Count > 0)
            {
                string currentReplicaDir = dirsToProcess.Pop();
                string relativePath = Path.GetRelativePath(replicaPath, currentReplicaDir);
                string expectedSourceDir = relativePath == "."
                    ? sourcePath
                    : Path.Combine(sourcePath, relativePath);

                if (!_fileSystem.DirectoryExists(expectedSourceDir))
                {
                    _fileSystem.DeleteDirectoryRecursively(currentReplicaDir);
                    _logger.LogInfo($"Deleted directory '{currentReplicaDir}' (REDUNDANT DIRECTORY).");
                    continue;
                }
                _logger.LogInfo($"Directory '{currentReplicaDir}' exists in source. Checking its contents.");

                var filesInReplicaDir = _fileSystem.GetFiles(currentReplicaDir);
                foreach (var replicaFilePath in filesInReplicaDir)
                {
                    string fileName = Path.GetFileName(replicaFilePath);
                    string expectedSourceFilePath = Path.Combine(expectedSourceDir, fileName);

                    if (!_fileSystem.FileExists(expectedSourceFilePath))
                    {
                        _fileSystem.DeleteFile(replicaFilePath);
                        _logger.LogInfo($"Deleted file '{replicaFilePath}' (REDUNDANT FILE).");
                    }
                    else
                        _logger.LogInfo($"File '{replicaFilePath}' exists in source.");
                }

                try
                {
                    var subDirs = _fileSystem.GetDirectories(currentReplicaDir);
                    foreach (var subDir in subDirs)
                        dirsToProcess.Push(subDir);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to get directories from '{currentReplicaDir}'. Skipping this directory.", ex);
                    continue;
                }
            }
        }
    }

}
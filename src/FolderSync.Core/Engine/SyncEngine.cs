using FolderSync.Core.FileUtils;
using FolderSync.Core.Logger;

namespace FolderSync.Core
{
    public class SyncEngine
    {
        private readonly ISyncLogger _logger;
        private readonly IFileSystem _fileUtils;

        public SyncEngine(ISyncLogger logger, IFileSystem fileUtils)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileUtils = fileUtils ?? throw new ArgumentNullException(nameof(fileUtils));
        }

        public void Synchronize(string sourcePath, string destinationPath)
        {
            if (string.IsNullOrEmpty(sourcePath))
                throw new ArgumentException("Value cannot be null or empty.", nameof(sourcePath));
            if (string.IsNullOrEmpty(destinationPath))
                throw new ArgumentException("Value cannot be null or empty.", nameof(destinationPath));

            _logger.LogInfo($"Starting synchronization from '{sourcePath}' to '{destinationPath}'.");

            // Synchronization logic goes here

            _logger.LogInfo("Synchronization completed.");
        }
    }

}
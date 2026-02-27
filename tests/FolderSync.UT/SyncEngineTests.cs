using Moq;
using FolderSync.Core.Engine;
using FolderSync.Core.Logger;
using FolderSync.Core.FileUtils;

namespace FolderSync.UT
{
    [TestClass]
    public class SyncEngineTests
    {
        private Mock<IFileSystem> _fileSystemMock = null!;
        private Mock<ISyncLogger> _loggerMock = null!;
        private SyncEngine _engine = null!;

        [TestInitialize]
        public void Setup()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            _loggerMock = new Mock<ISyncLogger>();
            _engine = new SyncEngine(_loggerMock.Object, _fileSystemMock.Object);
        }

        #region Tests for files
        
        [TestMethod]
        public void Synchronize_WhenNewFileInSource_ShouldCopyFileToReplica()
        {
            string sourcePath = "/Source";
            string replicaPath = "/Replica";
            string newFilePath = "/Source/file1.txt";

            _fileSystemMock.Setup(fs => fs.GetFiles(sourcePath))
                           .Returns(new[] { newFilePath });

            _fileSystemMock.Setup(fs => fs.GetDirectories(It.IsAny<string>()))
                           .Returns(Array.Empty<string>());

            _fileSystemMock.Setup(fs => fs.FileExists($"/Replica/file1.txt"))
                           .Returns(false);

            _engine.Synchronize(sourcePath, replicaPath);

            _fileSystemMock.Verify(fs => fs.CopyFileAndOverwrite(newFilePath, $"/Replica/file1.txt"), Times.Once);
        }

        [TestMethod]
        public void Synchronize_WhenRedundantFileReplica_ShouldDeleteFile()
        {
            string sourcePath = "/Source";
            string replicaPath = "/Replica";
            string oldFilePath = "/Replica/file2.txt";

            _fileSystemMock.Setup(fs => fs.DirectoryExists(sourcePath)).Returns(true);
            _fileSystemMock.Setup(fs => fs.DirectoryExists(replicaPath)).Returns(true);

            _fileSystemMock.Setup(fs => fs.GetFiles(replicaPath))
                           .Returns(new[] { oldFilePath });
            _fileSystemMock.Setup(fs => fs.GetDirectories(It.IsAny<string>()))
                           .Returns(Array.Empty<string>());
            _fileSystemMock.Setup(fs => fs.FileExists($"/Source/file2.txt"))
                           .Returns(false);

            _engine.Synchronize(sourcePath, replicaPath);

            _fileSystemMock.Verify(fs => fs.DeleteFile(oldFilePath), Times.Once);
        }

        [TestMethod]
        public void Synchronize_WhenFileInSourceWasModified_ShouldCopyFileToReplica()
        {
            string sourcePath = "/Source";
            string replicaPath = "/Replica";
            string changedSizeFilePath = "/Source/fileSize.txt";
            string changedDataTimeFilePath = "/Source/fileDataTime.txt";
            string changedMD5FilePath = "/Source/fileMD5.txt";
            string replicaChangedSizeFilePath = "/Replica/fileSize.txt";
            string replicaChangedDataTimeFilePath = "/Replica/fileDataTime.txt";
            string replicaChangedMD5FilePath = "/Replica/fileMD5.txt";

            _fileSystemMock.Setup(fs => fs.GetFiles(sourcePath))
                           .Returns(new[] { changedSizeFilePath, changedDataTimeFilePath, changedMD5FilePath });
            _fileSystemMock.Setup(fs => fs.GetFiles(replicaPath))
                           .Returns(new[] { replicaChangedSizeFilePath, replicaChangedDataTimeFilePath, replicaChangedMD5FilePath });

            _fileSystemMock.Setup(fs => fs.GetDirectories(It.IsAny<string>()))
                           .Returns(Array.Empty<string>());

            _fileSystemMock.Setup(fs => fs.FileExists(replicaChangedSizeFilePath))
                           .Returns(true);
            _fileSystemMock.Setup(fs => fs.FileExists(replicaChangedDataTimeFilePath))
                           .Returns(true);
            _fileSystemMock.Setup(fs => fs.FileExists(replicaChangedMD5FilePath))
                           .Returns(true);

            _fileSystemMock.Setup(fs => fs.GetFileDetails(changedSizeFilePath))
                           .Returns(new FileDetails(100, DateTime.UtcNow));
            _fileSystemMock.Setup(fs => fs.GetFileDetails(replicaChangedSizeFilePath))
                           .Returns(new FileDetails(50, DateTime.UtcNow));
            _fileSystemMock.Setup(fs => fs.GetFileDetails(changedDataTimeFilePath))
                            .Returns(new FileDetails(100, DateTime.UtcNow.AddMinutes(1)));
            _fileSystemMock.Setup(fs => fs.GetFileDetails(replicaChangedDataTimeFilePath))
                            .Returns(new FileDetails(100, DateTime.UtcNow));
            _fileSystemMock.Setup(fs => fs.GetFileDetails(changedMD5FilePath))
                            .Returns(new FileDetails(100, DateTime.UtcNow));
            _fileSystemMock.Setup(fs => fs.GetFileDetails(replicaChangedMD5FilePath))
                            .Returns(new FileDetails(100, DateTime.UtcNow));
            _fileSystemMock.Setup(fs => fs.FileOpenRead(changedMD5FilePath))
                            .Returns(new MemoryStream(new byte[] { 1, 2, 3 }));
            _fileSystemMock.Setup(fs => fs.FileOpenRead(replicaChangedMD5FilePath))
                            .Returns(new MemoryStream(new byte[] { 4, 5, 6 }));

            _engine.Synchronize(sourcePath, replicaPath);

            _fileSystemMock.Verify(fs => fs.CopyFileAndOverwrite(changedSizeFilePath, replicaChangedSizeFilePath), Times.Once);
            _fileSystemMock.Verify(fs => fs.CopyFileAndOverwrite(changedDataTimeFilePath, replicaChangedDataTimeFilePath), Times.Once);
            _fileSystemMock.Verify(fs => fs.CopyFileAndOverwrite(changedMD5FilePath, replicaChangedMD5FilePath), Times.Once);
        }
        #endregion

        #region Tests for directories

        [TestMethod]
        public void Synchronize_WhenNewDirInSource_ShouldCreateDirInReplica()
        {
            string sourcePath = "/Source";
            string replicaPath = "/Replica";
            string newDirPath = "/Source/NewFolder";

            _fileSystemMock.Setup(fs => fs.GetDirectories(sourcePath)).Returns(new[] { newDirPath });
            _fileSystemMock.Setup(fs => fs.GetDirectories(replicaPath)).Returns(Array.Empty<string>());

            _fileSystemMock.Setup(fs => fs.DirectoryExists("/Replica/NewFolder")).Returns(false);

            _engine.Synchronize(sourcePath, replicaPath);

            _fileSystemMock.Verify(fs => fs.CreateDirectory("/Replica/NewFolder"), Times.Once);
        }

        [TestMethod]
        public void Synchronize_WhenRedundantDirInReplica_ShouldDeleteDir()
        {
            string sourcePath = "/Source";
            string replicaPath = "/Replica";
            string oldDirPath = "/Replica/OldFolder";

            _fileSystemMock.Setup(fs => fs.DirectoryExists(sourcePath)).Returns(true);
            _fileSystemMock.Setup(fs => fs.DirectoryExists(replicaPath)).Returns(true);

            _fileSystemMock.Setup(fs => fs.GetDirectories(replicaPath)).Returns(new[] { oldDirPath });
            _fileSystemMock.Setup(fs => fs.GetDirectories(sourcePath)).Returns(Array.Empty<string>());

            _fileSystemMock.Setup(fs => fs.DirectoryExists("/Source/OldFolder")).Returns(false);

            _engine.Synchronize(sourcePath, replicaPath);

            _fileSystemMock.Verify(fs => fs.DeleteDirectoryRecursively(oldDirPath), Times.Once);
        }

        #endregion
    }
}
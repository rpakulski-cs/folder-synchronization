using FolderSync.Core.FileUtils;
using FolderSync.Core.Logger;
using FolderSync.Core.Engine;

namespace FolderSync.App
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string sourcePath = string.Empty;
            string replicaPath = string.Empty;
            string logPath = string.Empty;
            int intervalSeconds = 0;

            (sourcePath, replicaPath, logPath, intervalSeconds) = ValidateArguments(args);

            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(replicaPath) ||
                string.IsNullOrEmpty(logPath) || intervalSeconds <= 0)
            {
                Console.WriteLine("Error: Invalid or missing arguments.");
                Console.WriteLine("Usage: FolderSync.App --source <path> --replica <path> --interval <seconds> --log <path>");
                return;
            }

            LocalFileSystem fileSystem = new LocalFileSystem();
            SyncLogger logger = new SyncLogger(logPath);
            SyncEngine engine = new SyncEngine(logger, fileSystem);

            Console.WriteLine($"[INIT] Sync engine configured. Interval: {intervalSeconds}s. Press Ctrl+C to exit.");

            await RunFolderSynchronizationAsync(engine, intervalSeconds, sourcePath, replicaPath);
        }

        private static async Task RunFolderSynchronizationAsync(SyncEngine engine, int intervalSeconds, string sourcePath, string replicaPath)
        {
            engine.Synchronize(sourcePath, replicaPath);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));

            try
            {
                while (await timer.WaitForNextTickAsync())
                {
                    engine.Synchronize(sourcePath, replicaPath);
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"Synchronization was interrupted. Exiting application.  ");
            }
        }

        private static (string sourcePath, string replicaPath, string logPath, int intervalSeconds) ValidateArguments(string[] args)
        {
            string sourcePath = string.Empty;
            string replicaPath = string.Empty;
            string logPath = string.Empty;
            int intervalSeconds = 0;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "--source":
                        if (i + 1 < args.Length)
                        {
                            sourcePath = args[i + 1];
                            i++; 
                        }
                        break;
                    case "--replica":
                        if (i + 1 < args.Length)
                        {
                            replicaPath = args[i + 1];
                            i++;
                        }
                        break;
                    case "--log":
                        if (i + 1 < args.Length)
                        {
                            logPath = args[i + 1];
                            i++;
                        }
                        break;
                    case "--interval":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int seconds))
                        {
                            intervalSeconds = seconds;
                            i++;
                        }
                        break;
                }
            }

            return (sourcePath, replicaPath, logPath, intervalSeconds);
        }
    }
}
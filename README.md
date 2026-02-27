# Folder Synchronization Tool

A robust, cross-platform command-line utility built in **.NET 8** that performs one-way folder synchronization. It periodically synchronizes the contents of a source folder to a replica folder, ensuring they are identical.

This project was developed as a technical assessment/portfolio piece demonstrating Clean Architecture, SOLID principles, and advanced Unit Testing in C#.

## Features

* **One-Way Synchronization:** Guarantees the replica folder perfectly matches the source folder.
* **Deep Change Detection:** Detects file modifications by comparing **File Size**, **Last Write Time**, and **MD5 Checksums**.
* **Orphan Removal:** Automatically cleans up files and directories in the replica that no longer exist in the source.
* **Periodic Execution:** Runs continuously in the background at a user-defined interval using  **PeriodicTimer**.
* **Comprehensive Logging:** Logs file creation, copying, and deletion operations to both the Console and a specified log file.
* **Resilient I/O Operations:** Handles access violations and locked files gracefully without crashing the main sync loop.

## Architecture & Technical Decisions

The project is strictly divided into logical layers to follow **Clean Architecture** and **SOLID** principles:

1. **`FolderSync.Core` (Domain/Engine/FileSystem):**
   Contains the core business logic (`SyncEngine`). It has absolutely no direct dependencies on the physical hard drive or console. It communicates with the outside world purely through abstractions (`IFileSystem`, `ISyncLogger`). 
2. **`FolderSync.App` (Presentation/Entry Point):**
   The console application. It acts as the composition root, parsing CLI arguments, instantiating the dependencies (Poor Man's DI), and starting the periodic loop.

## Tests

The solution includes a dedicated `FolderSync.UT` project using **MSTest** and **Moq**.

Tests cover core synchronization scenarios:
* Copying new files/directories to the replica.
* Deleting redundant files/directories from the replica.
* Overwriting modified files (detecting MD5/Size/modifiedDate changes).

To run the test suite:
```bash
dotnet test
```

## Usage

### Prerequisites
* **.NET 8 SDK** installed on your machine.

### Command Line Arguments
The application requires 4 arguments to run:
* `--source` : Path to the source directory.
* `--replica`: Path to the replica directory (will be created if it doesn't exist).
* `--interval`: Synchronization interval in seconds.
* `--log`    : Path to the log file.

### Example Run (Windows)
```bash
dotnet run --project FolderSync.App -- --source "C:\Source" --replica "C:\Replica" --interval 60 --log "C:\sync.log"
```

### Example Run (macOS / Linux)
```bash
dotnet run --project FolderSync.App -- --source "/Users/Name/Source" --replica "/Users/Name/Replica" --interval 60 --log "/Users/Name/sync.log"
```

To exit the application, press `Ctrl+C`.
namespace SyncUp.Agent.Application.Watcher.Abstractions;

public interface IFileSystemWatcherWrapper : IDisposable
{
    string Path { get; set; }
    string Filter { get; set; }
    bool IncludeSubdirectories { get; set; }
    int InternalBufferSize { get; set; }
    NotifyFilters NotifyFilter { get; set; }
    bool EnableRaisingEvents { get; set; }

    event FileSystemEventHandler? Created;
    event FileSystemEventHandler? Deleted;
    event RenamedEventHandler? Renamed;
    event ErrorEventHandler? Error;
}

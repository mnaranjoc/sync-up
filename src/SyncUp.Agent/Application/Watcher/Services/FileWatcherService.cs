using System.Net;
using SyncUp.Agent.Application.Synchronization.Queue.FileEvent;
using SyncUp.Agent.Application.Synchronization.Services;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.Watcher.Services;

public class FileWatcherService : IFileWatcherService, IDisposable
{
    private FileSystemWatcher? _watcher;
    private readonly ISynchronizationService _service;
    private readonly ILogger<FileWatcherService> _logger;
    private readonly object _lock = new();
    private bool _disposed;

    public FileWatcherService(ISynchronizationService service, ILogger<FileWatcherService> logger)
    {
        _service = service;
        _logger = logger;
    }

    public void Start(string path)
    {
        path = Files.FixFilePath(path);

        lock (_lock)
        {
            if (_watcher != null) return;

            if (string.IsNullOrEmpty(path)) throw new Exception(Constants.PATH_NOT_PROVIDED);

            if (!Directory.Exists(path)) throw new Exception(Constants.FOLDER_DOESNT_EXIST);

            _watcher = new FileSystemWatcher(path)
            {
                Filter = Constants.FILTER_ALL_FILES,
                IncludeSubdirectories = true,
                InternalBufferSize = 65536,
                NotifyFilter = NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
            };

            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Renamed += OnRenamed;
            _watcher.Error += OnError;

            _watcher.EnableRaisingEvents = true;
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (_watcher == null) return;

            _watcher.EnableRaisingEvents = false;

            _watcher.Created -= OnCreated;
            _watcher.Deleted -= OnDeleted;
            _watcher.Renamed -= OnRenamed;
            _watcher.Error -= OnError;

            _watcher.Dispose();

            _watcher = null;
        }
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        var fileEvent = new AddFile() { Name = e.Name, FullPath = e.FullPath };
        _service.SubmitChange(fileEvent);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        var fileEvent = new RemoveFile() { Name = e.Name };
        _service.SubmitChange(fileEvent);
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        var fileEvent = new RenameFile() { OldName = e.OldName, Name = e.Name };
        _service.SubmitChange(fileEvent);
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        var ex = e.GetException();
        if (ex != null)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Stop();
        }

        _disposed = true;
    }
}

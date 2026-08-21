using SyncUp.Agent.Application.Synchronization.Queue.FileEvent;
using SyncUp.Agent.Application.Synchronization.Services;
using SyncUp.Agent.Application.Watcher.Abstractions;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.Watcher.Services;

public class FileWatcherService : IFileWatcherService, IDisposable
{
    private readonly ISynchronizationService _service;
    private readonly IFileSystemWatcherWrapper _watcher;
    private readonly ILogger<FileWatcherService> _logger;
    private readonly object _lock = new();
    private bool _isActive;
    private bool _disposed;

    public FileWatcherService(ISynchronizationService service, IFileSystemWatcherWrapper watcher, ILogger<FileWatcherService> logger)
    {
        _watcher = watcher;
        _service = service;
        _logger = logger;
    }

    public bool IsActive() => _isActive;

    public void Start(string path)
    {
        lock (_lock)
        {
            if (IsActive()) return;

            var fixedPath = Files.FixFilePath(path);

            if (string.IsNullOrEmpty(fixedPath))
                throw new Exception(Constants.PATH_NOT_PROVIDED);

            if (!Directory.Exists(fixedPath))
                throw new Exception(Constants.FOLDER_DOESNT_EXIST);

            _watcher.Path = fixedPath;
            _watcher.Filter = Constants.FILTER_ALL_FILES;
            _watcher.IncludeSubdirectories = true;
            _watcher.InternalBufferSize = 65536;
            _watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName;

            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Renamed += OnRenamed;
            _watcher.Error += OnError;

            _watcher.EnableRaisingEvents = true;
            _isActive = true;
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (!_isActive) return;

            _watcher.EnableRaisingEvents = false;

            _watcher.Created -= OnCreated;
            _watcher.Deleted -= OnDeleted;
            _watcher.Renamed -= OnRenamed;
            _watcher.Error -= OnError;

            _isActive = false;
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
            _watcher.Dispose();
        }

        _disposed = true;
    }
}

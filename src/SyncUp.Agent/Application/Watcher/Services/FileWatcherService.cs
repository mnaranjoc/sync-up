using SyncUp.Agent.Application.Synchronization.Queue;
using SyncUp.Agent.Application.Synchronization.Queue.Operations;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.Watcher.Services;

public class FileWatcherService : IFileWatcherService, IDisposable
{
    private FileSystemWatcher? _watcher;
    private readonly ISynchronizationQueue _queue;
    private readonly ILogger<FileWatcherService> _logger;
    private readonly object _lock = new();
    private bool _disposed;

    public FileWatcherService(ISynchronizationQueue queue, ILogger<FileWatcherService> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    public void Start(string path)
    {
        lock (_lock)
        {
            if (_watcher != null) return;

            if (string.IsNullOrEmpty(path)) throw new Exception(Constants.PATH_NOT_PROVIDED);

            if (!Directory.Exists(path))
            {
                if (path.StartsWith("~/"))
                {
                    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    path = Path.Combine(home, path.Substring(2));
                }
                
                if (!Directory.Exists(path))
                    throw new Exception(Constants.FOLDER_DOESNT_EXIST);
            }

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
        var operation = new AddFile() { Name = e.Name, FullPath = e.FullPath };
        _queue.Queue(operation);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        var operation = new RemoveFile() { Name = e.Name };
        _queue.Queue(operation);
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        var operation = new RenameFile() { OldName = e.OldName, Name = e.Name };
        _queue.Queue(operation);
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

using SyncUp.Agent.Common;

namespace SyncUp.Agent.Services.FileWatcherService;

public class FileWatcherService : IFileWatcherService
{
    private FileSystemWatcher _watcher;
    private readonly ILogger<FileWatcherService> _logger;

    public FileWatcherService(ILogger<FileWatcherService> logger)
    {
        _watcher = new FileSystemWatcher("")
        {
            Filter = Constants.FILTER_ALL_FILES,
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.Attributes
                             | NotifyFilters.CreationTime
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
                             | NotifyFilters.LastAccess
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Security
                             | NotifyFilters.Size
        };

        _watcher.Changed += OnChanged;
        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;
        _watcher.Error += OnError;

        _logger = logger;
    }

    public void Start()
    {
        if (_watcher != null)
            _watcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation($"Changed: {e.FullPath}");
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation($"Created: {e.FullPath}");
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation($"Deleted: {e.FullPath}");
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        _logger.LogInformation($"Renamed: {e.OldFullPath} -> {e.FullPath}");
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        var ex = e.GetException();
        if (ex != null)
        {
            _logger.LogInformation($"Message: {ex.Message}");
            _logger.LogInformation($"Stacktrace: {ex.StackTrace}");
            var iex = ex.InnerException;
            if (iex != null)
            {
                _logger.LogInformation($"Message: {iex.Message}");
                _logger.LogInformation($"Stacktrace: {iex.StackTrace}");
            }
        }
    }
}

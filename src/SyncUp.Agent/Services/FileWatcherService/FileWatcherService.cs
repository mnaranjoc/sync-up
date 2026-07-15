using SyncUp.Agent.Common;
using SyncUp.Agent.Services.AgentFilesService;

namespace SyncUp.Agent.Services.FileWatcherService;

public class FileWatcherService : IFileWatcherService, IDisposable
{
    private FileSystemWatcher? _watcher;
    private readonly IAgentFilesService _agentFilesService;
    private readonly ILogger<FileWatcherService> _logger;
    private readonly object _lock = new();
    private bool _disposed;

    public FileWatcherService(IAgentFilesService agentFilesService, ILogger<FileWatcherService> logger)
    {
        _agentFilesService = agentFilesService;
        _logger = logger;
    }

    public void Start(string path)
    {
        lock (_lock)
        {
            if (_watcher != null) return;

            if (string.IsNullOrEmpty(path)) throw new Exception("Path was not provided");

            if (!Directory.Exists(path))
            {
                if (path.StartsWith("~/"))
                {
                    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    path = Path.Combine(home, path.Substring(2));
                }
                
                if (!Directory.Exists(path))
                    throw new Exception("Directory does not exist");
            }

            _watcher = new FileSystemWatcher(path)
            {
                Filter = Constants.FILTER_ALL_FILES,
                IncludeSubdirectories = true,
                InternalBufferSize = 65536,
                NotifyFilter = NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
            };

            _watcher.Changed += OnChanged;
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

            _watcher.Changed -= OnChanged;
            _watcher.Created -= OnCreated;
            _watcher.Deleted -= OnDeleted;
            _watcher.Renamed -= OnRenamed;
            _watcher.Error -= OnError;

            _watcher.Dispose();

            _watcher = null;
        }
    }

    private async void OnChanged(object sender, FileSystemEventArgs e)
        => await _agentFilesService.UpdateFile(e.FullPath);

    private async void OnCreated(object sender, FileSystemEventArgs e)
        => await _agentFilesService.AddFile(e.FullPath);

    private async void OnDeleted(object sender, FileSystemEventArgs e)
        => await _agentFilesService.RemoveFile(e.FullPath);

    private async void OnRenamed(object sender, RenamedEventArgs e)
        => await _agentFilesService.RenameFile(e.OldFullPath, e.FullPath);

    private void OnError(object sender, ErrorEventArgs e)
    {
        var ex = e.GetException();
        if (ex != null)
        {
            _logger.LogError(ex, "Error en FileSystemWatcher: {Message}", ex.Message);
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

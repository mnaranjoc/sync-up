using SyncUp.Agent.Common;

namespace SyncUp.Agent.Service;

public class WatcherService : BackgroundService
{
    private FileSystemWatcher _watcher;
    private readonly ILogger<WatcherService> _logger;

    public WatcherService(ILogger<WatcherService> logger)
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{WorkerName} started.", nameof(WatcherService));

        try
        {
            _watcher.EnableRaisingEvents = true;

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("{WorkerName} stopped.", nameof(WatcherService));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.ERROR_UNEXPECTED);
        }
        finally
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

using SyncUp.Agent.Common;
using SyncUp.Agent.Services.FileWatcherService;

namespace SyncUp.Agent.Services;

public class WatcherTask : BackgroundService
{
    private readonly IFileWatcherService _fileWatcherService;
    private readonly ILogger<WatcherTask> _logger;

    public WatcherTask(IFileWatcherService fileWatcherService, ILogger<WatcherTask> logger)
    {
        _fileWatcherService = fileWatcherService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{WorkerName} started.", nameof(WatcherTask));

        try
        {
            _fileWatcherService.Start();

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("{WorkerName} stopped.", nameof(WatcherTask));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.ERROR_UNEXPECTED);
        }
        finally
        {
            _fileWatcherService.Stop();
        }
    }
}

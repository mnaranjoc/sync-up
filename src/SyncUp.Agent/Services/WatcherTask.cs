using SyncUp.Agent.Common;
using SyncUp.Agent.Services.FileWatcherService;

namespace SyncUp.Agent.Services;

public class WatcherTask : BackgroundService
{
    private readonly IFileWatcherService _fileWatcherService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WatcherTask> _logger;

    public WatcherTask(IFileWatcherService fileWatcherService, IConfiguration configuration, ILogger<WatcherTask> logger)
    {
        _fileWatcherService = fileWatcherService;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{WorkerName} started.", nameof(WatcherTask));

        string? watchDirectory = _configuration["WatchDirectory"];
        if (string.IsNullOrWhiteSpace(watchDirectory))
        {
            _logger.LogCritical("Configuration Error: 'PathToWatch' is missing from appsettings.json.");
            return;
        }

        try
        {
            _fileWatcherService.Start(watchDirectory);

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

using SyncUp.Agent.Application.Watcher.Services;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.SyncUp;

public class WatcherTask : BackgroundService
{
    private readonly IFileWatcherService _fileWatcherService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WatcherTask> _logger;
    private readonly string watchDirectory;

    public WatcherTask(IFileWatcherService fileWatcherService, IConfiguration configuration, ILogger<WatcherTask> logger)
    {
        _fileWatcherService = fileWatcherService ?? throw new ArgumentNullException(nameof(fileWatcherService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        watchDirectory = _configuration?["WatchDirectory"] ?? throw new ArgumentNullException(nameof(watchDirectory));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _fileWatcherService.Start(watchDirectory);

            _logger.LogInformation("{WorkerName} started.", nameof(WatcherTask));

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.ERROR_UNEXPECTED);
        }
        finally
        {
            _fileWatcherService.Stop();

            _logger.LogInformation("{WorkerName} stopped.", nameof(WatcherTask));
        }
    }
}

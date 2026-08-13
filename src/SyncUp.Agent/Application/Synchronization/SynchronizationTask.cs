using SyncUp.Agent.Application.Synchronization.Services;

namespace SyncUp.Agent.Application.Synchronization;

public class SynchronizationTask : BackgroundService
{
    private readonly ISynchronizationService _service;

    public SynchronizationTask(ISynchronizationService service)
    {
        _service = service;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_service.IsOutOfSync())
            {
                await _service.SynchronizeAsync(stoppingToken);
            }
        }
    }
}

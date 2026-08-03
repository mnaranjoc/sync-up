using SyncUp.Agent.Application.SyncUp.Services;

namespace SyncUp.Agent.Application.SyncUp;

public class SyncUpTask : BackgroundService
{
    private readonly ISyncUpService _service;

    public SyncUpTask(ISyncUpService service)
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

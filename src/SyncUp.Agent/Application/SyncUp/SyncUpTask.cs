using SyncUp.Agent.Application.SyncUp.Services;
using SyncUp.Shared.Enums;

namespace SyncUp.Agent.Application.SyncUp
{
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
                if (_service.GetSyncStatus() != SyncStatus.InSync)
                {
                    await _service.SynchronizeAsync();
                }
            }
        }
    }
}

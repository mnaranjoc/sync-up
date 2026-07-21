using SyncUp.Agent.Application.SyncUp.Services;

namespace SyncUp.Agent.Application.SyncUp
{
    public class SyncUpTask : BackgroundService
    {
        private readonly ISyncUpService _service;
        private readonly ILogger<SyncUpTask> _logger;

        public SyncUpTask(ISyncUpService service, ILogger<SyncUpTask> logger)
        {
            _service = service;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SyncTask running at: {time}", DateTimeOffset.Now);

            // Get server files
            var serverFiles = await _service.GetServerFilesList();
            _logger.LogInformation("Server has {Count} files.", serverFiles?.Count);

            // Get agent files
            var agentFiles = _service.GetAgentFilesList();
            _logger.LogInformation("Agent has {Count} files.", agentFiles?.Count);

            while (!stoppingToken.IsCancellationRequested)
            {
                await _service.SynchronizeAsync();
            }
        }
    }
}

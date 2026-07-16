using SyncUp.Agent.Application.SyncUp.Services.SyncUpService;
using SyncUp.Shared.Models;

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
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("SyncTask running at: {time}", DateTimeOffset.Now);

                // Get server files
                var serverFiles = await _service.GetServerFilesList();
                if (serverFiles != null)
                {
                    _logger.LogInformation("Server has {Count} files.", serverFiles.Count);
                }

                await Task.Delay(5000, stoppingToken);

                // Get agent files
                var agentFiles = await _service.GetAgentFilesList();
                if (agentFiles != null)
                {
                    _logger.LogInformation("Agent has {Count} files.", agentFiles.Count);
                }

                // Get missing items in one or another list
                if (serverFiles != null && agentFiles != null)
                {
                    List<FileEntry> missingFiles = serverFiles
                        .Concat(agentFiles)
                        .DistinctBy(f => f.Path)
                        .ToList();
                }
            }
        }
    }
}

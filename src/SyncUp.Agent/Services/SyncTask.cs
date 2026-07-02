using SyncUp.Agent.Services.SyncUpService;
using System.Text.Json;

namespace SyncUp.Agent.Services
{
    public class SyncTask : BackgroundService
    {
        private readonly ISyncUpService _service;
        private readonly ILogger<SyncTask> _logger;

        public SyncTask(ISyncUpService service, ILogger<SyncTask> logger)
        {
            _service = service;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("SyncTask running at: {time}", DateTimeOffset.Now);

                var serverFiles = await _service.GetServerFilesList();
                if (serverFiles != null)
                {
                    var serverFilesJson = JsonSerializer.Serialize(serverFiles);
                    _logger.LogInformation(serverFilesJson);
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}

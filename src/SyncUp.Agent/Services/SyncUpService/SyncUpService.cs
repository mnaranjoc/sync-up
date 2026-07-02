using SyncUp.Shared.Models;

namespace SyncUp.Agent.Services.SyncUpService
{
    public class SyncUpService : ISyncUpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SyncUpService> _logger;

        public SyncUpService(HttpClient httpClient, ILogger<SyncUpService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<FileEntry>> GetServerFilesList()
        {
            var response = await _httpClient.GetAsync("sync-manager/files");
            _logger.LogInformation(await response.Content.ReadAsStringAsync());

            return new List<FileEntry>();
        }
    }
}

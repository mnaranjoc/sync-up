using System.Net.Http.Json;
using SyncUp.Agent.Common;
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

        public async Task<List<FileEntry>?> GetAgentFilesList()
        {
            return null;
        }

        public async Task<List<FileEntry>?> GetServerFilesList()
        {
            try
            {
                var response = await _httpClient.GetAsync("sync-manager/files");
                
                var files = await response.Content.ReadFromJsonAsync<List<FileEntry>>();

                return files;
            }
            catch (HttpRequestException)
            {
                _logger.LogError(Constants.ERROR_SERVER_LIST);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Constants.ERROR_UNEXPECTED);
            }

            return null;
        }
    }
}

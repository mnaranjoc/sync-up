using SyncUp.Shared.Models;
using System.Net.Http.Json;

namespace SyncUp.Agent.Infrastructure.Api
{
    public class SyncUpApiClient : ISyncUpApiClient
    {
        private readonly HttpClient _httpClient;

        public SyncUpApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<FileEntry>> GetFilesAsync()
        {
            using var response = await _httpClient.GetAsync("sync-manager/files");
            response.EnsureSuccessStatusCode();

            var files = await response.Content.ReadFromJsonAsync<List<FileEntry>>();
            return files ?? [];
        }

        public async Task AddFileAsync(MultipartFormDataContent content)
        {
            using var response = await _httpClient.PostAsync("sync-manager/file", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task RenameFileAsync(string path, HttpContent content)
        {
            using var response = await _httpClient.PutAsync($"sync-manager/file/{path}/rename", content);
            response.EnsureSuccessStatusCode();
        }
    }
}

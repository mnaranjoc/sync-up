using SyncUp.Shared.Models;
using System.Net.Http.Json;

namespace SyncUp.Agent.Infrastructure.Api
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<FileEntry>> GetFilesAsync(CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync("sync-manager/files", cancellationToken);
            response.EnsureSuccessStatusCode();

            var files = await response.Content.ReadFromJsonAsync<List<FileEntry>>();
            return files ?? [];
        }

        public async Task<FileEntry?> GetFileAsync(string name, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync($"sync-manager/file/{name}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var file = await response.Content.ReadFromJsonAsync<FileEntry>();
            return file ?? null;
        }

        public async Task AddFileAsync(MultipartFormDataContent content, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.PostAsync("sync-manager/file", content, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task RenameFileAsync(string name, HttpContent content, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.PutAsync($"sync-manager/file/{name}/rename", content, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveFileAsync(string name, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.DeleteAsync($"sync-manager/file/{name}", cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}

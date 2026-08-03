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

        public async Task<List<FileEntry>> GetFilesAsync()
        {
            using var response = await _httpClient.GetAsync("sync-manager/files");
            response.EnsureSuccessStatusCode();

            var files = await response.Content.ReadFromJsonAsync<List<FileEntry>>();
            return files ?? [];
        }

        public async Task<FileEntry?> GetFileAsync(string name)
        {
            using var response = await _httpClient.GetAsync($"sync-manager/file/{name}");
            response.EnsureSuccessStatusCode();

            var file = await response.Content.ReadFromJsonAsync<FileEntry>();
            return file ?? null;
        }

        public async Task AddFileAsync(MultipartFormDataContent content)
        {
            using var response = await _httpClient.PostAsync("sync-manager/file", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task RenameFileAsync(string name, HttpContent content)
        {
            using var response = await _httpClient.PutAsync($"sync-manager/file/{name}/rename", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveFileAsync(string name)
        {
            using var response = await _httpClient.DeleteAsync($"sync-manager/file/{name}");
            response.EnsureSuccessStatusCode();
        }
    }
}

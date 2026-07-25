using SyncUp.Shared.Models;

namespace SyncUp.Agent.Infrastructure.Api
{
    public interface IApiClient
    {
        public Task<List<FileEntry>> GetFilesAsync();

        public Task AddFileAsync(MultipartFormDataContent content);

        public Task RenameFileAsync(string name, HttpContent content);

        public Task RemoveFileAsync(string name);
    }
}

using SyncUp.Shared.Models;

namespace SyncUp.Agent.Infrastructure.Api
{
    public interface ISyncUpApiClient
    {
        public Task<List<FileEntry>> GetFilesAsync();

        public Task AddFileAsync(MultipartFormDataContent content);

        public Task RenameFileAsync(string path, HttpContent content);
    }
}

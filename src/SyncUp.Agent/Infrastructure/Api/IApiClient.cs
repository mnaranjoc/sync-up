using SyncUp.Shared.Models;

namespace SyncUp.Agent.Infrastructure.Api
{
    public interface IApiClient
    {
        public Task<List<FileEntry>> GetFilesAsync(CancellationToken cancellationToken);

        public Task<FileEntry?> GetFileAsync(string name, CancellationToken cancellationToken);

        public Task AddFileAsync(MultipartFormDataContent content, CancellationToken cancellationToken);

        public Task RenameFileAsync(string name, HttpContent content, CancellationToken cancellationToken);

        public Task RemoveFileAsync(string name, CancellationToken cancellationToken);
    }
}

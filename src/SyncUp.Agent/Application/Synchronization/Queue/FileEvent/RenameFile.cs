using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Models;
using SyncUp.Shared.Util;
using System.Text;
using System.Text.Json;

namespace SyncUp.Agent.Application.Synchronization.Queue.FileEvent
{
    public class RenameFile : IFileEvent
    {
        public string? Name { get; set; } = "";

        public string? FullPath { get; set; } = "";

        public string? OldName { get; set; } = "";

        public async Task ExecuteAsync(IApiClient apiClient, CancellationToken cancellationToken)
        {
            try
            {
                var fileEntry = new FileEntry() { Name = Name };

                var renameFileRequest = JsonSerializer.Serialize(fileEntry);

                using var content = new StringContent(
                    renameFileRequest,
                    Encoding.UTF8,
                    "application/json"
                );

                await apiClient.RenameFileAsync($"{OldName}", content, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(Constants.ERROR_SERVER_RENAME, ex);
            }
            catch (Exception ex)
            {
                throw new Exception(Constants.ERROR_UNEXPECTED, ex);
            }
        }
    }
}

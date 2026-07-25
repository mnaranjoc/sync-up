using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Models;
using SyncUp.Shared.Util;
using System.Text;
using System.Text.Json;

namespace SyncUp.Agent.Application.Synchronization.Queue.Operations
{
    public class RenameFile : IOperation
    {
        public string? Name { get; set; } = "";

        public string? FullPath { get; set; } = "";

        public string? OldName { get; set; } = "";

        public Task ExecuteAsync(IApiClient apiClient)
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

                return apiClient.RenameFileAsync(OldName, content);
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

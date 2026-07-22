using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Models;
using System.Text;
using System.Text.Json;

namespace SyncUp.Agent.Application.Synchronization.Queue.Operations
{
    public class RenameFile : IOperation
    {
        public string Path { get; set; } = "";

        public string OldPath { get; set; } = "";

        public Task ExecuteAsync(ISyncUpApiClient apiClient)
        {
            var fileEntry = new FileEntry() { Path = Path };

            var renameFileRequest = JsonSerializer.Serialize(fileEntry);            

            using var content = new StringContent(
                renameFileRequest,
                Encoding.UTF8,
                "application/json"
            );

            return apiClient.RenameFileAsync(OldPath, content);
        }
    }
}

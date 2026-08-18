using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.Synchronization.Queue.FileEvent
{
    public class GetFile : IFileEvent
    {
        public string? Name { get; set; } = "";

        public string? FullPath { get; set; } = "";

        public string? OldName { get; set; } = "";

        public int Delay { get; set; }

        public async Task ExecuteAsync(IApiClient apiClient, CancellationToken cancellationToken)
        {
            try
            {
                var file = await apiClient.GetFileAsync($"{Name}", cancellationToken);

                if (file != null)
                {
                    var path = Files.FixFilePath($"{FullPath}");
                    await File.WriteAllTextAsync(path, string.Empty, cancellationToken);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.Synchronization.Queue.FileEvent
{
    public class GetFile : IFileEvent
    {
        public string? Name { get; set; } = "";

        public string? FullPath { get; set; } = "";

        public string? OldName { get; set; } = "";

        public async Task ExecuteAsync(IApiClient apiClient)
        {
            try
            {
                var file = await apiClient.GetFileAsync($"{Name}");

                if (file != null)
                {
                    await File.WriteAllTextAsync($"{FullPath}", string.Empty);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(Constants.ERROR_SERVER_GET, ex);
            }
            catch (Exception ex)
            {
                throw new Exception(Constants.ERROR_UNEXPECTED, ex);
            }
        }
    }
}

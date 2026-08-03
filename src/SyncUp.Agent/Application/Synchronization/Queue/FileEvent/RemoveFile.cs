using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.Synchronization.Queue.FileEvent
{
    internal class RemoveFile : IFileEvent
    {
        public string? Name { get; set; } = "";

        public string? FullPath { get; set; } = "";

        public string? OldName { get; set; } = "";

        public Task ExecuteAsync(IApiClient apiClient, CancellationToken cancellationToken)
        {
            try
            {
                return apiClient.RemoveFileAsync(Name, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(Constants.ERROR_SERVER_REMOVE, ex);
            }
            catch (Exception ex)
            {
                throw new Exception(Constants.ERROR_UNEXPECTED, ex);
            }
        }
    }
}

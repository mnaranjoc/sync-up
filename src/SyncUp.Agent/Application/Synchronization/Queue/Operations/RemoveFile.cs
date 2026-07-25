using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.Synchronization.Queue.Operations
{
    internal class RemoveFile : IOperation
    {
        public string? Name { get; set; } = "";

        public string? FullPath { get; set; } = "";

        public string? OldName { get; set; } = "";

        public Task ExecuteAsync(IApiClient apiClient)
        {
            try
            {
                return apiClient.RemoveFileAsync(Name);
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

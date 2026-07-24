using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.Synchronization.Queue.Operations
{
    internal class RemoveFile : IOperation
    {
        public string Path { get; set; } = "";

        public string OldPath { get; set; } = "";

        public Task ExecuteAsync(ISyncUpApiClient apiClient)
        {
            try
            {
                return apiClient.RemoveFileAsync(Path);
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

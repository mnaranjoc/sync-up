using SyncUp.Agent.Infrastructure.Api;

namespace SyncUp.Agent.Application.Synchronization.Queue.Operations
{
    public interface IOperation
    {
        public string Path { get; set; }

        public string OldPath { get; set; }

        Task ExecuteAsync(ISyncUpApiClient apiClient);
    }
}

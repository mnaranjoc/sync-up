using SyncUp.Agent.Infrastructure.Api;

namespace SyncUp.Agent.Application.Synchronization.Queue.Operations
{
    public interface IOperation
    {
        public string? Name { get; set; }

        public string? FullPath { get; set; }

        public string? OldName { get; set; }

        Task ExecuteAsync(IApiClient apiClient);
    }
}

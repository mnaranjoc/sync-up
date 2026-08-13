using SyncUp.Agent.Infrastructure.Api;

namespace SyncUp.Agent.Application.Synchronization.Queue.FileEvent
{
    public interface IFileEvent
    {
        public string? Name { get; set; }

        public string? FullPath { get; set; }

        public string? OldName { get; set; }

        public int Delay { get; set; }

        Task ExecuteAsync(IApiClient apiClient, CancellationToken cancellationToken);
    }
}

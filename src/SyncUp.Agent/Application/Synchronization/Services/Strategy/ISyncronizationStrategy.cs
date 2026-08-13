using SyncUp.Shared.Enums;

namespace SyncUp.Agent.Application.Synchronization.Services.Strategy;

public interface ISynchronizationStrategy
{
    public SyncStatus SyncStatus { get; }

    Task<SyncStatus> RunAsync(CancellationToken cancellationToken);
}

using SyncUp.Shared.Enums;

namespace SyncUp.Agent.Application.Synchronization.Services.Strategy;

public class InSyncStrategy : ISynchronizationStrategy
{
    public SyncStatus SyncStatus => SyncStatus.InSync;

    public Task<SyncStatus> RunAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

using SyncUp.Agent.Application.Synchronization.Queue;
using SyncUp.Agent.Application.Synchronization.Queue.FileEvent;
using SyncUp.Agent.Application.Synchronization.Services.Strategy;
using SyncUp.Shared.Enums;

namespace SyncUp.Agent.Application.Synchronization.Services;

public class SynchronizationService : ISynchronizationService
{
    private readonly IEnumerable<ISynchronizationStrategy> _strategies;
    private readonly ISynchronizationQueue _queue;

    private SyncStatus _syncStatus;
    

    public SynchronizationService(IEnumerable<ISynchronizationStrategy> strategies, ISynchronizationQueue queue)
    {
        _strategies = strategies;
        _queue = queue;
    }

    public SyncStatus GetSyncStatus()
    {
        return _syncStatus;
    }

    public void SetSyncStatus(SyncStatus syncStatus)
    {
        _syncStatus = syncStatus;
    }

    public bool IsOutOfSync()
    {
        return GetSyncStatus() != SyncStatus.InSync;
    }

    public void SubmitChange(IFileEvent fileEvent)
    {
        _queue.Queue(fileEvent);

        SetSyncStatus(SyncStatus.OutOfSync);
    }

    public async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
        var strategy = _strategies.FirstOrDefault(s => s.SyncStatus == _syncStatus);

        if (strategy == null)
            return;

        var updatedStatus = await strategy.RunAsync(cancellationToken);

        SetSyncStatus(updatedStatus);
    }
}

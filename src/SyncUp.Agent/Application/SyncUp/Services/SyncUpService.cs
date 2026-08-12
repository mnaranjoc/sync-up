using SyncUp.Agent.Application.Synchronization.Queue;
using SyncUp.Agent.Application.Synchronization.Queue.FileEvent;
using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Enums;
using SyncUp.Shared.Models;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.SyncUp.Services;

public class SyncUpService : ISyncUpService
{
    private readonly IEnumerable<ISynchronizationStrategy> _strategies;
    private readonly ISynchronizationQueue _queue;

    private SyncStatus _syncStatus;
    

    public SyncUpService(IEnumerable<ISynchronizationStrategy> strategies, ISynchronizationQueue queue)
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

        await strategy.RunAsync(cancellationToken);

        RefreshSyncStatusFromQueue();
    }

    private void RefreshSyncStatusFromQueue()
    {
        if (_queue.IsQueueEmpty())
            SetSyncStatus(SyncStatus.InSync);
        else
            SetSyncStatus(SyncStatus.OutOfSync);
    }
}

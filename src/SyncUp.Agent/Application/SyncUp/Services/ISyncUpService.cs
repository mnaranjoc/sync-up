using SyncUp.Agent.Application.Synchronization.Queue.FileEvent;
using SyncUp.Shared.Enums;
using SyncUp.Shared.Models;

namespace SyncUp.Agent.Application.SyncUp.Services;

public interface ISyncUpService
{
    public SyncStatus GetSyncStatus();

    public void SetSyncStatus(SyncStatus syncStatus);

    public bool IsOutOfSync();

    public void SubmitChange(IFileEvent fileEvent);

    public Task SynchronizeAsync(CancellationToken cancellationToken);
}

using SyncUp.Agent.Application.Synchronization.Queue.FileEvent;
using SyncUp.Shared.Enums;

namespace SyncUp.Agent.Application.Synchronization.Services;

public interface ISynchronizationService
{
    public SyncStatus GetSyncStatus();

    public void SetSyncStatus(SyncStatus syncStatus);

    public void SubmitChange(IFileEvent fileEvent);

    public Task SynchronizeAsync(CancellationToken cancellationToken);
}

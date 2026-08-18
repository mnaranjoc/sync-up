using SyncUp.Agent.Application.Synchronization.Queue;
using SyncUp.Agent.Application.Synchronization.Queue.FileEvent;
using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Enums;

namespace SyncUp.Agent.Application.Synchronization.Services.Strategy;

public class OutOfSyncStrategy : ISynchronizationStrategy
{
    private readonly ISynchronizationQueue _queue;
    private readonly IApiClient _apiClient;
    public SyncStatus SyncStatus => SyncStatus.OutOfSync;
    
    public OutOfSyncStrategy(ISynchronizationQueue queue, IApiClient apiClient)
    {
        _queue = queue;
        _apiClient = apiClient;
    }

    public async Task<SyncStatus> RunAsync(CancellationToken cancellationToken)
    {
        var fileEvents = _queue.DequeueAll();
        List<IFileEvent> failedItems = [];

        foreach (var fileEvent in fileEvents)
        {
            try
            {
                await Task.Delay(fileEvent.Delay, cancellationToken);

                await fileEvent.ExecuteAsync(_apiClient, cancellationToken);
            }
            catch (Exception)
            {
                SetFailedItem(failedItems, fileEvent);
            }
        }

        _queue.EnqueueAll(failedItems);

        return _queue.IsQueueEmpty() ? SyncStatus.InSync : SyncStatus.OutOfSync;
    }

    private static void SetFailedItem(List<IFileEvent> failedEvents, IFileEvent fileEvent)
    {
        var delay = fileEvent.Delay == 0 ? 1000 : fileEvent.Delay * 2;
        delay = Math.Min(delay, 5000);
        fileEvent.Delay = delay;

        failedEvents.Add(fileEvent);
    }
}

using SyncUp.Agent.Application.Synchronization.Queue;
using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Enums;

namespace SyncUp.Agent.Application.SyncUp.Services;

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

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var fileEvents = _queue.DequeueAll();

        if (fileEvents.Count > 0)
        {
            foreach (var fileEvent in fileEvents)
                await fileEvent.ExecuteAsync(_apiClient, cancellationToken);
        }
    }
}

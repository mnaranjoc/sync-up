using System;
using SyncUp.Shared.Enums;

namespace SyncUp.Agent.Application.SyncUp.Services;

public class InSyncStrategy : ISynchronizationStrategy
{
    public SyncStatus SyncStatus => SyncStatus.InSync;

    public Task RunAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

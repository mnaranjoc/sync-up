using System;
using SyncUp.Shared.Enums;

namespace SyncUp.Agent.Application.SyncUp.Services;

public interface ISynchronizationStrategy
{
    public SyncStatus SyncStatus { get; }

    Task RunAsync(CancellationToken cancellationToken);
}

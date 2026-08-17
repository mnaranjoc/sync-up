using Moq;
using SyncUp.Agent.Application.Synchronization;
using SyncUp.Agent.Application.Synchronization.Services;

namespace SyncUp.Agent.Tests.Application.Synchronization;

public class SynchronizationTaskTests
{
    private readonly Mock<ISynchronizationService> _serviceMock;
    private readonly SynchronizationTask _task;

    public SynchronizationTaskTests()
    {
        _serviceMock = new Mock<ISynchronizationService>();
        _task = new SynchronizationTask(_serviceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallSynchronizeAsync_WhenTokenIsNotCancelled()
    {
        using var cts = new CancellationTokenSource();

        _serviceMock
            .Setup(s => s.SynchronizeAsync(It.IsAny<CancellationToken>()))
            .Callback(() => cts.Cancel())
            .Returns(Task.CompletedTask);

        await _task.StartAsync(cts.Token);

        if (_task.ExecuteTask != null)
            await _task.ExecuteTask;

        _serviceMock.Verify(s => s.SynchronizeAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotCallSynchronizeAsync_WhenCancellationIsRequested()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await _task.StartAsync(cts.Token);

        _serviceMock.Verify(s => s.SynchronizeAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

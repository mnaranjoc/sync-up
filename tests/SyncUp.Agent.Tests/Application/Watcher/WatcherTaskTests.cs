using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SyncUp.Agent.Application.SyncUp;
using SyncUp.Agent.Application.Watcher.Services;

namespace SyncUp.Agent.Tests.Application.Watcher
{
    public class WatcherTaskTests
    {
        private readonly Mock<IFileWatcherService> _fileWatcherServiceMock;
        private readonly IConfiguration _configuration;
        private readonly Mock<ILogger<WatcherTask>> _loggerMock;
        private readonly WatcherTask _task;

        public WatcherTaskTests()
        {
            var settings = new Dictionary<string, string> { { "WatchDirectory", "C:\\Users\\Test" } };

            _fileWatcherServiceMock = new Mock<IFileWatcherService>();
            _configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            _loggerMock = new Mock<ILogger<WatcherTask>>();
            _task = new WatcherTask(_fileWatcherServiceMock.Object, _configuration, _loggerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldStartServiceAndThenStop_WhenCancellationIsRequested()
        {
            using var cts = new CancellationTokenSource();

            _fileWatcherServiceMock
                .Setup(s => s.Start(It.IsAny<string>()))
                .Callback(() => cts.Cancel());

            await _task.StartAsync(cts.Token);

            if (_task.ExecuteTask != null)
                await _task.ExecuteTask;

            _fileWatcherServiceMock.Verify(s => s.Start(It.IsAny<string>()), Times.Once);
            _fileWatcherServiceMock.Verify(s => s.Stop(), Times.Once);
        }
    }
}

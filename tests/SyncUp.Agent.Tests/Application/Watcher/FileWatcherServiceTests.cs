using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SyncUp.Agent.Application.Synchronization.Queue.FileEvent;
using SyncUp.Agent.Application.Synchronization.Services;
using SyncUp.Agent.Application.Watcher.Abstractions;
using SyncUp.Agent.Application.Watcher.Services;

namespace SyncUp.Agent.Tests.Application.Watcher
{
    public class FileWatcherServiceTests : IDisposable
    {
        private readonly Mock<ISynchronizationService> _synchronizationServiceMoq;
        private readonly Mock<IFileSystemWatcherWrapper> _fileSystemWatcherWrapperMoq;
        private readonly Mock<ILogger<FileWatcherService>> _loggerMock;
        private readonly IFileWatcherService _fileWatcherService;
        private readonly string _tmpDirectory;

        public FileWatcherServiceTests()
        {
            _synchronizationServiceMoq = new Mock<ISynchronizationService>();
            _fileSystemWatcherWrapperMoq = new Mock<IFileSystemWatcherWrapper>();
            _loggerMock = new Mock<ILogger<FileWatcherService>>();
            _fileWatcherService = new FileWatcherService(_synchronizationServiceMoq.Object, _fileSystemWatcherWrapperMoq.Object, _loggerMock.Object);

            _tmpDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tmpDirectory);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tmpDirectory))
                Directory.Delete(_tmpDirectory, true);
        }

        [Fact]
        public void IsActive_ShouldReturnFalse_WhenNotStarted()
        {
            _fileWatcherService.IsActive().Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("<invalid-directory>")]
        public void Start_ShouldThrow_WhenPathIsNotValid(string path)
        {
            Action act = () => _fileWatcherService.Start(path);

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Start_ShouldStartWatcher_WhenPathIsValid()
        {
            _fileWatcherService.Start(_tmpDirectory);

            _fileWatcherService.IsActive().Should().BeTrue();
        }

        [Fact]
        public void Start_ShouldDoNothing_WhenAlreadyStarted()
        {
            _fileWatcherService.Start(_tmpDirectory);

            Action act = () => _fileWatcherService.Start(_tmpDirectory);

            act.Should().NotThrow();
        }

        [Fact]
        public void Stop_ShouldDisposeWatcher_WhenStarted()
        {
            _fileWatcherService.Start(_tmpDirectory);

            _fileWatcherService.Stop();

            _fileWatcherService.IsActive().Should().BeFalse();
        }

        [Fact]
        public void Stop_ShouldDoNothing_WhenNotStarted()
        {
            Action act = () => _fileWatcherService.Stop();

            act.Should().NotThrow();
        }

        [Fact]
        public void OnCreated_ShouldSubmitAddFileEvent_WhenFileCreated()
        {
            _fileWatcherService.Start(_tmpDirectory);
            var eventArgs = new FileSystemEventArgs(WatcherChangeTypes.Created, _tmpDirectory, "test.txt");

            _fileSystemWatcherWrapperMoq.Raise(f => f.Created += null, eventArgs);

            _synchronizationServiceMoq.Verify(s => s.SubmitChange(It.Is<AddFile>(e =>
                e.Name == "test.txt" && e.FullPath == Path.Combine(_tmpDirectory, "test.txt")
            )), Times.Once);
        }

        [Fact]
        public void OnDeleted_ShouldSubmitRemoveFileEvent_WhenFileDeleted()
        {
            _fileWatcherService.Start(_tmpDirectory);
            var eventArgs = new FileSystemEventArgs(WatcherChangeTypes.Deleted, _tmpDirectory, "test.txt");

            _fileSystemWatcherWrapperMoq.Raise(w => w.Deleted += null, eventArgs);

            _synchronizationServiceMoq.Verify(s => s.SubmitChange(It.Is<RemoveFile>(e =>
                e.Name == "test.txt"
            )), Times.Once);
        }

        [Fact]
        public void OnRenamed_ShouldSubmitRenameFileEvent_WhenFileRenamed()
        {
            _fileWatcherService.Start(_tmpDirectory);
            var eventArgs = new RenamedEventArgs(WatcherChangeTypes.Renamed, _tmpDirectory, "new.txt", "old.txt");

            _fileSystemWatcherWrapperMoq.Raise(w => w.Renamed += null, eventArgs);

            _synchronizationServiceMoq.Verify(s => s.SubmitChange(It.Is<RenameFile>(e =>
                e.Name == "new.txt" && e.OldName == "old.txt"
            )), Times.Once);
        }

        [Fact]
        public void OnError_ShouldLogError_WhenErrorOccurs()
        {
            _fileWatcherService.Start(_tmpDirectory);
            var exception = new Exception("Unhandled exception");
            var eventArgs = new ErrorEventArgs(exception);

            _fileSystemWatcherWrapperMoq.Raise(w => w.Error += null, eventArgs);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Dispose_ShouldStopAndDisposeWatcher()
        {
            _fileWatcherService.Start(_tmpDirectory);

            ((IDisposable)_fileWatcherService).Dispose();

            _fileWatcherService.IsActive().Should().BeFalse();
            _fileSystemWatcherWrapperMoq.Verify(w => w.Dispose(), Times.Once);
        }
    }
}

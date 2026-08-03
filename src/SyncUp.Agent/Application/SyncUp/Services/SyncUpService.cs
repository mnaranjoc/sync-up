using SyncUp.Agent.Application.Synchronization.Queue;
using SyncUp.Agent.Application.Synchronization.Queue.FileEvent;
using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Enums;
using SyncUp.Shared.Models;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.SyncUp.Services;

public class SyncUpService : ISyncUpService
{
    private readonly ISynchronizationQueue _queue;
    private readonly IApiClient _apiClient;
    private readonly IConfiguration _config;
    private readonly ILogger<SyncUpService> _logger;

    private readonly string _agentWatchDirectory;
    private readonly List<FileEntry> _agentFilesList = new List<FileEntry>();

    private SyncStatus _syncStatus;
    private bool _firstTime = true;

    public SyncUpService(ISynchronizationQueue queue, IApiClient apiClient, IConfiguration config, ILogger<SyncUpService> logger)
    {
        _queue = queue;
        _apiClient = apiClient;
        _config = config;
        _logger = logger;

        _agentWatchDirectory = $"{_config[Constants.CONFIG_WATCH_DIRECTORY]}";
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

    public IReadOnlyList<FileEntry>? GetAgentFilesList()
    {
        if (_firstTime)
        {
            var files = Files.GetFilesFromDirectory(_agentWatchDirectory);

            foreach (string fullPath in files)
            {
                var name = Path.GetFileName(fullPath);

                _agentFilesList.Add(
                    new FileEntry()
                    {
                        Name = name,
                        Source = Location.Agent,
                        FullPath = fullPath
                    }
                );
            }

            _firstTime = false;
        }

        return _agentFilesList;
    }

    private async Task<IReadOnlyList<FileEntry>?> GetServerFilesList(CancellationToken cancellationToken)
    {
        try
        {
            return await _apiClient.GetFilesAsync(cancellationToken);
        }
        catch (HttpRequestException)
        {
            _logger.LogError(Constants.ERROR_SERVER_LIST);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.ERROR_UNEXPECTED);
        }

        return null;
    }

    public IList<FileEntry> GetSyncDifferences(IReadOnlyList<FileEntry> serverFiles, IReadOnlyList<FileEntry> agentFiles)
    {
        if (serverFiles == null || agentFiles == null)
            return [];

        var serverDifferences = serverFiles.ExceptBy(
            agentFiles.Select(a => a.Name), s => s.Name);

        var agentDifferences = agentFiles.ExceptBy(
            serverFiles.Select(s => s.Name), a => a.Name);

        var differences = serverDifferences.Concat(agentDifferences).ToList();

        return differences;
    }

    public async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
        if (_syncStatus == SyncStatus.Unknown)
            await InitializeSyncQueueAsync(cancellationToken);

        else if (_syncStatus == SyncStatus.OutOfSync)
            await ProcessQueueAsync(cancellationToken);
    }

    private async Task InitializeSyncQueueAsync(CancellationToken cancellationToken)
    {
        if (_syncStatus != SyncStatus.Unknown)
            return;

        var serverFiles = await GetServerFilesList(cancellationToken);
        var agentFiles = GetAgentFilesList();
        var differences = GetSyncDifferences(serverFiles, agentFiles);

        if (differences.Count > 0)
        {
            foreach (var file in differences)
            {
                IFileEvent? fileEvent = null;

                if (file.Source == Location.Server)
                {
                    var fileName = $"{file.Name}";
                    var fullPath = Path.Combine(_agentWatchDirectory, fileName);

                    fileEvent = new GetFile() { Name = fileName, FullPath = fullPath };
                }

                if (file.Source == Location.Agent)
                {
                    var fileName = $"{file.Name}";
                    var fullPath = $"{file.FullPath}";

                    fileEvent = new AddFile() { Name = fileName, FullPath = fullPath };
                }

                if (fileEvent != null)
                    SubmitChange(fileEvent);
            }

            RefreshSyncStatusFromQueue();
        }
    }

    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        var fileEvents = _queue.DequeueAll();

        if (fileEvents.Count > 0)
        {
            foreach (var fileEvent in fileEvents)
                await fileEvent.ExecuteAsync(_apiClient, cancellationToken);

            RefreshSyncStatusFromQueue();
        }
    }

    private void RefreshSyncStatusFromQueue()
    {
        if (_queue.IsQueueEmpty())
            SetSyncStatus(SyncStatus.InSync);
        else
            SetSyncStatus(SyncStatus.OutOfSync);
    }
}

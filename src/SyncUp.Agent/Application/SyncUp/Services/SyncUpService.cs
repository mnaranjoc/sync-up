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

    private readonly List<FileEntry> _agentFilesList = new List<FileEntry>();

    private SyncStatus _syncStatus;
    private bool _firstTime = true;

    public SyncUpService(ISynchronizationQueue queue, IApiClient apiClient, IConfiguration config, ILogger<SyncUpService> logger)
    {
        _queue = queue;
        _apiClient = apiClient;
        _config = config;
        _logger = logger;
    }

    public SyncStatus GetSyncStatus()
    {
        return _syncStatus;
    }

    public void SetSyncStatus(SyncStatus syncStatus)
    {
        _syncStatus = syncStatus;
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
            string dir = $"{_config[Constants.CONFIG_WATCH_DIRECTORY]}";
            var files = Files.GetFilesFromDirectory(dir);

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

    public async Task<IReadOnlyList<FileEntry>?> GetServerFilesList()
    {
        try
        {
            return await _apiClient.GetFilesAsync();
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

    public async Task SynchronizeAsync()
    {
        if (_syncStatus == SyncStatus.Unknown)
            await InitializeSyncQueueAsync();

        else if (_syncStatus == SyncStatus.OutOfSync)
            await ProcessQueueAsync();
    }

    private async Task InitializeSyncQueueAsync()
    {
        if (_syncStatus != SyncStatus.Unknown)
            return;

        var serverFiles = await GetServerFilesList();
        var agentFiles = GetAgentFilesList();
        var differences = GetSyncDifferences(serverFiles, agentFiles);

        if (differences.Count > 0)
        {
            foreach (var file in differences)
            {
                IFileEvent? fileEvent = null;

                if (file.Source == Location.Agent)
                    fileEvent = new AddFile() { Name = file.Name, FullPath = file.FullPath };

                if (fileEvent != null)
                    SubmitChange(fileEvent);
            }

            RefreshSyncStatusFromQueue();
        }
    }

    private async Task ProcessQueueAsync()
    {
        var fileEvents = _queue.DequeueAll();

        if (fileEvents.Count > 0)
        {
            foreach (var fileEvent in fileEvents)
                await fileEvent.ExecuteAsync(_apiClient);

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

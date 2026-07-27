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

    public async Task InitializeSyncQueueAsync()
    {
        if (_syncStatus != SyncStatus.Unknown)
            return;

        // Get server files
        var serverFiles = await GetServerFilesList();
        _logger.LogInformation("Server has {Count} files.", serverFiles?.Count);

        // Get agent files
        var agentFiles = GetAgentFilesList();
        _logger.LogInformation("Agent has {Count} files.", agentFiles?.Count);
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
                _agentFilesList.Add(new FileEntry() { Name = name, FullPath = fullPath });

                var fileEvent = new AddFile() { Name = name, FullPath = fullPath };
                _queue.Queue(fileEvent);
            }

            _firstTime = false;
        }

        return _agentFilesList;
    }

    public async Task<List<FileEntry>?> GetServerFilesList()
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

    public async Task SynchronizeAsync()
    {
        if (_syncStatus == SyncStatus.Unknown)
        {
            await InitializeSyncQueueAsync();
        }
        else if (_syncStatus == SyncStatus.OutOfSync)
        {
            await ProcessQueueAsync();
        }

        if (_queue.IsQueueEmpty())
            SetSyncStatus(SyncStatus.InSync);
        else
            SetSyncStatus(SyncStatus.OutOfSync);
    }

    public async Task ProcessQueueAsync()
    {
        var filEvents = _queue.DequeueAll();

        foreach (var fileEvent in filEvents)
        {
            await fileEvent.ExecuteAsync(_apiClient);

            await Task.Delay(1000);
        }
    }
}

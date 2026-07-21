using SyncUp.Agent.Application.Synchronization.Queue;
using SyncUp.Agent.Application.Synchronization.Services;
using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Models;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.SyncUp.Services;

public class SyncUpService : ISyncUpService
{
    private readonly IFileService _agentFilesService;
    private readonly ISynchronizationQueue _queue;
    private readonly ISyncUpApiClient _apiClient;
    private readonly ILogger<SyncUpService> _logger;
    private bool firstTime = true;

    public SyncUpService(IFileService agentFilesService, ISynchronizationQueue queue, ISyncUpApiClient apiClient, ILogger<SyncUpService> logger)
    {
        _agentFilesService = agentFilesService;
        _queue = queue;
        _apiClient = apiClient;
        _logger = logger;
    }

    public IReadOnlyList<FileEntry>? GetAgentFilesList()
    {
        if (firstTime)
        {
            _agentFilesService.LoadFolderFilesOnStartup();
            firstTime = false;
        }

        return _agentFilesService.GetFiles();
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
        var operations = _queue.DequeueAll();

        foreach (var operation in operations)
        {
            await operation.ExecuteAsync(_apiClient);

            await Task.Delay(1000);
        }
    }
}

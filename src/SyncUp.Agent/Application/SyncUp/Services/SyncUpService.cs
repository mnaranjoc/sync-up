using SyncUp.Agent.Application.Synchronization.Queue;
using SyncUp.Agent.Application.Synchronization.Queue.Operations;
using SyncUp.Agent.Infrastructure.Api;
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

    private bool _firstTime = true;

    public SyncUpService(ISynchronizationQueue queue, IApiClient apiClient, IConfiguration config, ILogger<SyncUpService> logger)
    {
        _queue = queue;
        _apiClient = apiClient;
        _config = config;
        _logger = logger;
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

                var operation = new AddFile() { Name = name, FullPath = fullPath };
                _queue.Queue(operation);
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
        var operations = _queue.DequeueAll();

        foreach (var operation in operations)
        {
            await operation.ExecuteAsync(_apiClient);

            await Task.Delay(1000);
        }
    }
}

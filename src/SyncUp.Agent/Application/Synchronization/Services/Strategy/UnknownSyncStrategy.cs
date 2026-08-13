using SyncUp.Agent.Application.Synchronization.Queue;
using SyncUp.Agent.Application.Synchronization.Queue.FileEvent;
using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Enums;
using SyncUp.Shared.Models;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.Synchronization.Services.Strategy;

public class UnknownSyncStrategy : ISynchronizationStrategy
{
    private readonly ISynchronizationQueue _queue;
    private readonly IApiClient _apiClient;
    private readonly ILogger<SynchronizationService> _logger;
    public SyncStatus SyncStatus => SyncStatus.Unknown;
    private readonly string _agentWatchDirectory;

    public UnknownSyncStrategy(ISynchronizationQueue queue, IApiClient apiClient, IConfiguration config, ILogger<SynchronizationService> logger)
    {
        _queue = queue;
        _apiClient = apiClient;
        _agentWatchDirectory = $"{config[Constants.CONFIG_WATCH_DIRECTORY]}";
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);

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
                    _queue.Queue(fileEvent);
            }
        }
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

    public IReadOnlyList<FileEntry>? GetAgentFilesList()
    {
        List<FileEntry> _agentFilesList = new();

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

        return _agentFilesList;
    }

    public IList<FileEntry> GetSyncDifferences(IReadOnlyList<FileEntry>? serverFiles, IReadOnlyList<FileEntry>? agentFiles)
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
}

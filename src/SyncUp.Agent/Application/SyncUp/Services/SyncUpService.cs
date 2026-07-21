using SyncUp.Agent.Application.Synchronization.Services;
using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Models;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.SyncUp.Services;

public class SyncUpService : ISyncUpService
{
    private readonly IFileService _agentFilesService;
    private readonly ISyncUpApiClient _apiClient;
    private readonly ILogger<SyncUpService> _logger;
    private bool firstTime = true;

    public SyncUpService(IFileService agentFilesService, ISyncUpApiClient apiClient, ILogger<SyncUpService> logger)
    {
        _agentFilesService = agentFilesService;
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FileEntry>?> GetAgentFilesList()
    {
        if (firstTime)
        {
            await _agentFilesService.LoadFolderFilesOnStartup();
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
}

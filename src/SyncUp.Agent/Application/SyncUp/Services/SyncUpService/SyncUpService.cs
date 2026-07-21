using System.Net.Http.Json;
using SyncUp.Agent.Application.SyncUp.Services.AgentFilesService;
using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Models;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.SyncUp.Services.SyncUpService;

public class SyncUpService : ISyncUpService
{
    private readonly IAgentFilesService _agentFilesService;
    private readonly ISyncUpApiClient _apiClient;
    private readonly ILogger<SyncUpService> _logger;
    private bool firstTime = true;

    public SyncUpService(IAgentFilesService agentFilesService, ISyncUpApiClient apiClient, ILogger<SyncUpService> logger)
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

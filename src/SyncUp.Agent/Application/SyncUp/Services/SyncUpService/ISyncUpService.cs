using SyncUp.Shared.Models;

namespace SyncUp.Agent.Application.SyncUp.Services.SyncUpService;

public interface ISyncUpService
{
    public Task<IReadOnlyList<FileEntry>?> GetAgentFilesList();
    
    public Task<List<FileEntry>?> GetServerFilesList();
}

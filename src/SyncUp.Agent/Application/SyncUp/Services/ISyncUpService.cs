using SyncUp.Shared.Models;

namespace SyncUp.Agent.Application.SyncUp.Services;

public interface ISyncUpService
{
    public IReadOnlyList<FileEntry>? GetAgentFilesList();
    
    public Task<List<FileEntry>?> GetServerFilesList();

    public Task SynchronizeAsync();
}

using SyncUp.Shared.Models;

namespace SyncUp.Agent.Services.SyncUpService
{
    public interface ISyncUpService
    {
        public Task<IReadOnlyList<FileEntry>?> GetAgentFilesList();
        
        public Task<List<FileEntry>?> GetServerFilesList();
    }
}

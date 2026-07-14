using SyncUp.Shared.Models;

namespace SyncUp.Agent.Services.SyncUpService
{
    public interface ISyncUpService
    {
        public Task<List<FileEntry>?> GetAgentFilesList();
        
        public Task<List<FileEntry>?> GetServerFilesList();
    }
}

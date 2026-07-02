using SyncUp.Shared.Models;

namespace SyncUp.Agent.Services.SyncUpService
{
    public interface ISyncUpService
    {
        public Task<List<FileEntry>> GetServerFilesList();
    }
}

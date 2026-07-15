using SyncUp.Shared.Models;

namespace SyncUp.Agent.Services.AgentFilesService;

public interface IAgentFilesService
{
    public IReadOnlyList<FileEntry> GetFiles();

    public Task AddFile(string path);

    public Task RemoveFile(string path);

    public Task RenameFile(string oldPath, string newPath);

    public Task UpdateFile(string path);
}

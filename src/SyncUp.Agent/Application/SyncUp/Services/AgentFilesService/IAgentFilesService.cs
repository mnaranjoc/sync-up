using SyncUp.Shared.Models;

namespace SyncUp.Agent.Application.SyncUp.Services.AgentFilesService;

public interface IAgentFilesService
{
    public IReadOnlyList<FileEntry> GetFiles();

    public Task LoadFolderFilesOnStartup();

    public Task AddFile(string path);

    public Task RemoveFile(string path);

    public Task RenameFile(string oldPath, string newPath);
}

using SyncUp.Shared.Models;

namespace SyncUp.Agent.Application.Synchronization.Services;

public interface IFileService
{
    public IReadOnlyList<FileEntry> GetFiles();

    public void LoadFolderFilesOnStartup();

    public void AddFile(string path);

    public Task RemoveFile(string path);

    public Task RenameFile(string oldPath, string newPath);
}

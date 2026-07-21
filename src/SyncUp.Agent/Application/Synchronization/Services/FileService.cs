using SyncUp.Agent.Application.Synchronization.Queue;
using SyncUp.Agent.Application.Synchronization.Queue.Operations;
using SyncUp.Shared.Models;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.Synchronization.Services;

public class FileService : IFileService
{
    private readonly ISynchronizationQueue _queue;
    private readonly IConfiguration _configuration;
    private readonly List<FileEntry> _files = [];

    public FileService(ISynchronizationQueue queue, IConfiguration configuration)
    {
        _queue = queue;
        _configuration = configuration;
    }

    public IReadOnlyList<FileEntry> GetFiles()
        => _files;

    public void LoadFolderFilesOnStartup()
    {
        string dir = $"{_configuration[Constants.CONFIG_WATCH_DIRECTORY]}";
        var files = Files.GetFilesFromDirectory(dir);

        foreach (var path in files)
        {
            AddFile(path);
        }
    }

    private FileEntry? GetFile(string path)
        => _files.FirstOrDefault(x => x.Path == path);

    public void AddFile(string path)
    {
        var operation = new AddFile() { Path = path };
        _queue.Queue(operation);
    }

    public async Task RemoveFile(string path)
    {
        var itemToRemove = GetFile(path);

        if (itemToRemove != null)
            _files.Remove(itemToRemove);

        await Task.CompletedTask;
    }

    public Task RenameFile(string oldPath, string newPath)
    {
        var fileToRename = GetFile(oldPath);

        if (fileToRename != null)
        {
            fileToRename.Path = newPath;
        }

        return Task.CompletedTask;
    }
}

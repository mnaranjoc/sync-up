namespace SyncUp.Agent.Application.Watcher.Services.FileWatcherService;

public interface IFileWatcherService
{
    public void Start(string path);

    public void Stop();
}

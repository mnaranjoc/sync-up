namespace SyncUp.Agent.Application.Watcher.Services;

public interface IFileWatcherService
{
    public bool IsActive();

    public void Start(string path);

    public void Stop();
}

using System.Diagnostics.CodeAnalysis;

namespace SyncUp.Agent.Application.Watcher.Abstractions;

[ExcludeFromCodeCoverage]
public class FileSystemWatcherWrapper : IFileSystemWatcherWrapper
{
    private FileSystemWatcher? _watcher;

    public string Path
    {
        get => _watcher?.Path ?? string.Empty;
        set => EnsureWatcher().Path = value;
    }

    public string Filter
    {
        get => _watcher?.Filter ?? string.Empty;
        set => EnsureWatcher().Filter = value;
    }

    public bool IncludeSubdirectories
    {
        get => _watcher?.IncludeSubdirectories ?? false;
        set => EnsureWatcher().IncludeSubdirectories = value;
    }

    public int InternalBufferSize
    {
        get => _watcher?.InternalBufferSize ?? 8192;
        set => EnsureWatcher().InternalBufferSize = value;
    }

    public NotifyFilters NotifyFilter
    {
        get => _watcher?.NotifyFilter ?? (NotifyFilters)0;
        set => EnsureWatcher().NotifyFilter = value;
    }

    public bool EnableRaisingEvents
    {
        get => _watcher?.EnableRaisingEvents ?? false;
        set => EnsureWatcher().EnableRaisingEvents = value;
    }

    public event FileSystemEventHandler? Created
    {
        add => EnsureWatcher().Created += value;
        remove { if (_watcher != null) _watcher.Created -= value; }
    }

    public event FileSystemEventHandler? Deleted
    {
        add => EnsureWatcher().Deleted += value;
        remove { if (_watcher != null) _watcher.Deleted -= value; }
    }

    public event RenamedEventHandler? Renamed
    {
        add => EnsureWatcher().Renamed += value;
        remove { if (_watcher != null) _watcher.Renamed -= value; }
    }

    public event ErrorEventHandler? Error
    {
        add => EnsureWatcher().Error += value;
        remove { if (_watcher != null) _watcher.Error -= value; }
    }

    private FileSystemWatcher EnsureWatcher()
    {
        return _watcher ??= new FileSystemWatcher();
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _watcher = null;
    }
}

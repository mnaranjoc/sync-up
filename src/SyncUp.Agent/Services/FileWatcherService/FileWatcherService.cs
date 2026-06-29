using SyncUp.Agent.Common;

namespace SyncUp.Agent.Services.FileWatcherService;

public class FileWatcherService : IFileWatcherService, IDisposable
{
    private FileSystemWatcher? _watcher;
    private HttpClient _httpClient;
    private readonly ILogger<FileWatcherService> _logger;
    private readonly object _lock = new();
    private bool _disposed;

    public FileWatcherService(HttpClient httpClient, ILogger<FileWatcherService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public void Start(string path)
    {
        lock (_lock)
        {
            if (_watcher != null) return;

            _watcher = new FileSystemWatcher(path)
            {
                Filter = Constants.FILTER_ALL_FILES,
                IncludeSubdirectories = true,
                InternalBufferSize = 65536,
                NotifyFilter = NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
            };

            _watcher.Changed += OnChanged;
            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Renamed += OnRenamed;
            _watcher.Error += OnError;

            _watcher.EnableRaisingEvents = true;
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (_watcher == null) return;

            _watcher.EnableRaisingEvents = false;

            _watcher.Changed -= OnChanged;
            _watcher.Created -= OnCreated;
            _watcher.Deleted -= OnDeleted;
            _watcher.Renamed -= OnRenamed;
            _watcher.Error -= OnError;

            _watcher.Dispose();

            _watcher = null;
        }
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("Changed: {FullPath}", e.FullPath);
    }

    private async void OnCreated(object sender, FileSystemEventArgs e)
    {
        try
        {
            await ProcessAndUploadFileAsync(e.FullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical failure in OnCreated event handler for file: {FilePath}", e.FullPath);
        }
    }

    private async Task ProcessAndUploadFileAsync(string filePath)
    {
        FileStream? fileStream = await WaitForFileAccessAsync(filePath, maxRetries: 5, delayMs: 500);

        if (fileStream == null)
        {
            _logger.LogError("File is locked and could not be accessed after multiple attempts: {FilePath}", filePath);
            return;
        }

        try
        {
            using (fileStream)
            using (var streamContent = new StreamContent(fileStream))
            using (var content = new MultipartFormDataContent())
            {
                string fileName = Path.GetFileName(filePath);
                content.Add(streamContent, "file", fileName);

                var response = await _httpClient.PostAsync("sync-manager/file", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully uploaded {FileName}", fileName);
                }
                else
                {
                    _logger.LogError("Failed to upload {FileName}. Status code: {StatusCode}", fileName, response.StatusCode);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing and uploading file: {FilePath}", filePath);
        }
    }

    private async Task<FileStream?> WaitForFileAccessAsync(string filePath, int maxRetries, int delayMs)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                // Open the file with Read access and allow other processes to Read/Write
                return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch (IOException)
            {
                if (i == maxRetries - 1)
                {
                    break;
                }
                await Task.Delay(delayMs);
            }
        }
        return null;
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("Deleted: {FullPath}", e.FullPath);
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        _logger.LogInformation("Renamed: {OldFullPath} -> {FullPath}", e.OldFullPath, e.FullPath);
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        var ex = e.GetException();
        if (ex != null)
        {
            _logger.LogError(ex, "Error en FileSystemWatcher: {Message}", ex.Message);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Stop();
        }

        _disposed = true;
    }
}

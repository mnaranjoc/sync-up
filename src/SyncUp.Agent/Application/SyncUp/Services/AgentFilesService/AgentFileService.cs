using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Models;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.SyncUp.Services.AgentFilesService;

public class AgentFileService : IAgentFilesService
{
    private readonly ISyncUpApiClient _apiClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AgentFileService> _logger;
    private readonly List<FileEntry> _files = [];

    public AgentFileService(ISyncUpApiClient apiClient, IConfiguration configuration, ILogger<AgentFileService> logger)
    {
        _apiClient = apiClient;
        _configuration = configuration;
        _logger = logger;
    }

    public IReadOnlyList<FileEntry> GetFiles()
        => _files;

    public async Task LoadFolderFilesOnStartup()
    {
        string dir = $"{_configuration[Constants.CONFIG_WATCH_DIRECTORY]}";
        var files = Files.GetFilesFromDirectory(dir);

        foreach (var path in files)
        {
            await AddFile(path);
        }

        return;
    }

    private FileEntry? GetFile(string path)
        => _files.FirstOrDefault(x => x.Path == path);

    public async Task AddFile(string path)
    {
        try
        {
            await ProcessAndUploadFileAsync(path);

            var newFile = Files.FileEntryFromPath(path);

            _files.Add(newFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, path);
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

        using (fileStream)
        using (var streamContent = new StreamContent(fileStream))
        using (var content = new MultipartFormDataContent())
        {
            try
            {
                string fileName = Path.GetFileName(filePath);
                content.Add(streamContent, "file", fileName);

                await _apiClient.AddFileAsync(content);
            }
            catch (HttpRequestException)
            {
                _logger.LogError(Constants.ERROR_SERVER_UPLOAD);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Constants.ERROR_UNEXPECTED);
            }
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

using SyncUp.Shared.Models;

namespace SyncUp.Agent.Services.AgentFilesService;

public class AgentFileService : IAgentFilesService
{
    private HttpClient _httpClient;
    private readonly ILogger<AgentFileService> _logger;
    private readonly List<FileEntry> _files = [];

    public AgentFileService(HttpClient httpClient, ILogger<AgentFileService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public IReadOnlyList<FileEntry> GetFiles()
        => _files;

    private FileEntry? GetFile(string path)
        => _files.FirstOrDefault(x => x.Path == path);

    public async Task AddFile(string path)
    {
        try
        {
            //await ProcessAndUploadFileAsync(path);

            var newFile = new FileEntry()
            {
                Path = path,
                Sha256 = "sha256"
            };

            _files.Add(newFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical failure in OnCreated event handler for file: {FilePath}", path);
        }
    }

    // private async Task ProcessAndUploadFileAsync(string filePath)
    // {
    //     FileStream? fileStream = await WaitForFileAccessAsync(filePath, maxRetries: 5, delayMs: 500);

    //     if (fileStream == null)
    //     {
    //         _logger.LogError("File is locked and could not be accessed after multiple attempts: {FilePath}", filePath);
    //         return;
    //     }

    //     try
    //     {
    //         using (fileStream)
    //         using (var streamContent = new StreamContent(fileStream))
    //         using (var content = new MultipartFormDataContent())
    //         {
    //             string fileName = Path.GetFileName(filePath);
    //             content.Add(streamContent, "file", fileName);

    //             var response = await _httpClient.PostAsync("sync-manager/file", content);

    //             if (response.IsSuccessStatusCode)
    //             {
    //                 _logger.LogInformation("Successfully uploaded {FileName}", fileName);
    //             }
    //             else
    //             {
    //                 _logger.LogError("Failed to upload {FileName}. Status code: {StatusCode}", fileName, response.StatusCode);
    //             }
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error processing and uploading file: {FilePath}", filePath);
    //     }
    // }

    // private async Task<FileStream?> WaitForFileAccessAsync(string filePath, int maxRetries, int delayMs)
    // {
    //     for (int i = 0; i < maxRetries; i++)
    //     {
    //         try
    //         {
    //             // Open the file with Read access and allow other processes to Read/Write
    //             return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    //         }
    //         catch (IOException)
    //         {
    //             if (i == maxRetries - 1)
    //             {
    //                 break;
    //             }
    //             await Task.Delay(delayMs);
    //         }
    //     }
    //     return null;
    // }

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

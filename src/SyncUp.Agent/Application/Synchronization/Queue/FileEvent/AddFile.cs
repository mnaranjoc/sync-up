using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.Synchronization.Queue.FileEvent
{
    public class AddFile : IFileEvent
    {
        public string? Name { get; set; } = "";

        public string? FullPath { get; set; } = "";

        public string? OldName { get; set; } = "";

        public int Delay { get; set; }

        public async Task ExecuteAsync(IApiClient apiClient, CancellationToken cancellationToken)
        {
            FileStream? fileStream = await WaitForFileAccessAsync($"{FullPath}", maxRetries: 5, delayMs: 500, cancellationToken)
                ?? throw new Exception(Constants.ERROR_FILE_LOCKED);

            if (string.Equals(Name, "a.txt")) throw new Exception("TEST");

            using (fileStream)
            using (var streamContent = new StreamContent(fileStream))
            using (var content = new MultipartFormDataContent())
            {
                try
                {
                    content.Add(streamContent, "file", $"{Name}");
                    await apiClient.AddFileAsync(content, cancellationToken);
                }
                catch (HttpRequestException ex)
                {
                    throw new Exception(Constants.ERROR_SERVER_UPLOAD, ex);
                }
                catch (Exception ex)
                {
                    throw new Exception(Constants.ERROR_UNEXPECTED, ex);
                }
            }
        }

        private static async Task<FileStream?> WaitForFileAccessAsync(string fullPath, int maxRetries, int delayMs, CancellationToken cancellationToken)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                catch (IOException)
                {
                    if (i == maxRetries - 1)
                    {
                        break;
                    }
                    await Task.Delay(delayMs, cancellationToken);
                }
            }
            return null;
        }
    }
}

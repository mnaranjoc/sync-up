using SyncUp.Agent.Infrastructure.Api;
using SyncUp.Shared.Util;

namespace SyncUp.Agent.Application.Synchronization.Queue.Operations
{
    public class AddFile : IOperation
    {
        public string Path { get; set; } = "";

        public string OldPath { get; set; } = "";

        public async Task ExecuteAsync(ISyncUpApiClient apiClient)
        {
            FileStream? fileStream = await WaitForFileAccessAsync(Path, maxRetries: 5, delayMs: 500);

            if (fileStream == null)
                throw new Exception(Constants.ERROR_FILE_LOCKED);

            using (fileStream)
            using (var streamContent = new StreamContent(fileStream))
            using (var content = new MultipartFormDataContent())
            {
                try
                {
                    string fileName = System.IO.Path.GetFileName(Path);
                    content.Add(streamContent, "file", fileName);
                    await apiClient.AddFileAsync(content);
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

        private async Task<FileStream?> WaitForFileAccessAsync(string filePath, int maxRetries, int delayMs)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
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
    }
}

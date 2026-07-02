using SyncUp.Shared.Models;

namespace SyncUp.Server.Services
{
    public interface IFilesService
    {
        public IReadOnlyList<FileEntry> GetFiles();

        public FileEntry? GetFile(string path);

        public FileEntry? AddFile(IFormFile file);
    }
}

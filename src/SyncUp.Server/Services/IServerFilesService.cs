using SyncUp.Shared.Models;

namespace SyncUp.Server.Services
{
    public interface IServerFilesService
    {
        public IReadOnlyList<FileEntry> GetFiles();

        public FileEntry? GetFile(string path);

        public FileEntry? AddFile(IFormFile file);

        public FileEntry? RenameFile(string oldPath, string newPath);
    }
}

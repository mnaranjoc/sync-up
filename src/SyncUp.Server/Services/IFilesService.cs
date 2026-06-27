using SyncUp.Server.Models;

namespace SyncUp.Server.Services
{
    public interface IFilesService
    {
        public IReadOnlyList<FileEntry> GetFiles();

        public FileEntry? GetFile(string path);

        public void AddFile(FileEntry file);
    }
}

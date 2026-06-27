using SyncUp.Server.Models;

namespace SyncUp.Server.Services
{
    public class FilesService : IFilesService
    {
        private List<FileEntry> files =
        [
            new FileEntry() { Path = "images/beach.png", Sha256 = "ABC123" },
            new FileEntry() { Path = "images/house.png", Sha256 = "XYZ456" }
        ];

        public IReadOnlyList<FileEntry> GetFiles()
        {
            return files;
        }

        public FileEntry? GetFile(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            return files.FirstOrDefault(x => string.Equals(x.Path, path, StringComparison.Ordinal));
        }

        public void AddFile(FileEntry? file)
        {
            if (file == null)
                return;

            if (GetFile(file.Path) != null)
                return;

            files.Add(file);
        }
    }
}

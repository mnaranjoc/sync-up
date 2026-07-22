using SyncUp.Shared.Models;
using SyncUp.Shared.Util;

namespace SyncUp.Server.Services
{
    public class ServerFilesService : IServerFilesService
    {
        private readonly List<FileEntry> files = [];

        public IReadOnlyList<FileEntry> GetFiles()
            => files;

        public FileEntry? GetFile(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            return files.FirstOrDefault(x => string.Equals(x.Path, path, StringComparison.Ordinal));
        }

        public FileEntry? AddFile(IFormFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            var fileAlreadyExists = GetFile(file.FileName);
            if (fileAlreadyExists != null)
                return fileAlreadyExists;

            using var stream = file.OpenReadStream();
            var newFile = new FileEntry()
            {
                Path = file.FileName,
                Sha256 = Files.GetSHA256FromStream(stream)
            };

            files.Add(newFile);

            return newFile;
        }

        public FileEntry? RenameFile(string oldPath, string newPath)
        {
            var file = GetFile(oldPath);

            if (file == null)
                throw new ArgumentNullException(nameof(file));

            file.Path = newPath;

            return file;
        }
    }
}

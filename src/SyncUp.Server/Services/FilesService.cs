using SyncUp.Server.Models;
using System.Security.Cryptography;

namespace SyncUp.Server.Services
{
    public class FilesService : IFilesService
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

            using var stream = file.OpenReadStream();
            var hashBytes = SHA256.HashData(stream);
            var sha256 = Convert.ToHexString(hashBytes);

            var newFile = new FileEntry()
            {
                Path = file.FileName,
                Sha256 = sha256
            };

            if (GetFile(newFile.Path) != null)
                return null;

            files.Add(newFile);

            return newFile;
        }
    }
}

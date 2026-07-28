using SyncUp.Shared.Enums;
using SyncUp.Shared.Models;
using SyncUp.Shared.Util;

namespace SyncUp.Server.Services
{
    public class ServerFilesService : IServerFilesService
    {
        private readonly List<FileEntry> _files = [
            new FileEntry()
            {
                Name = "ServerFile.txt",
                Source = Location.Server,
                Sha256 = "559AEAD08264D5795D3909718CDD05ABD49572E84FE55590EEF31A88A08FDFFD"
            }
        ];

        public IReadOnlyList<FileEntry> GetFiles()
            => _files;

        public FileEntry? GetFile(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return _files.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.Ordinal));
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
                Name = file.FileName,
                Sha256 = Files.GetSHA256FromStream(stream)
            };

            _files.Add(newFile);

            return newFile;
        }

        public FileEntry? RenameFile(string oldName, string newName)
        {
            var file = GetFile(oldName);

            if (file == null)
                throw new ArgumentNullException(nameof(file));

            file.Name = newName;

            return file;
        }

        public void RemoveFile(string name)
        {
            var file = GetFile(name);

            if (file == null)
                throw new ArgumentNullException(nameof(file));

            _files.Remove(file);
        }
    }
}

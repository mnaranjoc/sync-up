using System.Security.Cryptography;
using SyncUp.Shared.Models;

namespace SyncUp.Shared.Util;

public class Files
{
    public static FileEntry FileEntryFromPath(string path)
    {
        using var stream = File.OpenRead(path);
        var newFile = new FileEntry()
        {
            Path = Path.GetFileName(path),
            Sha256 = Files.GetSHA256FromStream(stream)
        };

        return newFile;
    }
    
    public static string? GetSHA256FromStream(Stream stream)
    {
        string? result = null;

        var hashBytes = SHA256.HashData(stream);
        result = Convert.ToHexString(hashBytes);

        return result;
    }

    public static IEnumerable<string> GetFilesFromDirectory(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new Exception(Constants.PATH_NOT_PROVIDED);

        if (!Directory.Exists(path))
        {
            if (path.StartsWith("~/"))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                path = Path.Combine(home, path[2..]);
            }
            
            if (!Directory.Exists(path))
                throw new Exception(Constants.FOLDER_DOESNT_EXIST);
        }

        var files = Directory.GetFiles(path).Where(f => Path.GetFileName(f) != ".DS_Store");

        return files;
    }
}

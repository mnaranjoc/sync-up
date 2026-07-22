namespace SyncUp.Shared.Util;

public static class Constants
{
    public const string FILTER_ALL_FILES = "*.*";

    // Errors
    public const string ERROR_EMPTY_FILE = "File is empty.";
    public const string ERROR_FILE_LOCKED = "File is locked and could not be accessed after multiple attempts.";
    public const string ERROR_UNEXPECTED = "An unexpected error occurred.";
    public const string ERROR_SERVER_LIST = "An error ocurred while retrieving the server file list.";
    public const string ERROR_SERVER_UPLOAD = "An error ocurred while uploading the file.";
    public const string ERROR_SERVER_RENAMING = "New name was not provided to rename the file.";
    public const string PATH_NOT_PROVIDED = "Path was not provided.";
    public const string FOLDER_DOESNT_EXIST = "Folder doesnt exist.";
    
    // Config
    public const string CONFIG_ALLOW_EMPTY_FILES = "AllowEmptyFiles";
    public const string CONFIG_WATCH_DIRECTORY = "WatchDirectory";
}

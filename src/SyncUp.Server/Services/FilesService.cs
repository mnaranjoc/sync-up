namespace SyncUp.Server.Services
{
    public class FilesService : IFilesService
    {
        public List<string> GetAll()
        {
            return new List<string>()
            {
                "test1",
                "test2"
            };
        }
    }
}

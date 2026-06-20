using SyncUp.Agent.Services;
using SyncUp.Agent.Services.FileWatcherService;

namespace SyncUp.Agent;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSingleton<IFileWatcherService, FileWatcherService>();
        builder.Services.AddHostedService<WatcherTask>();

        var host = builder.Build();
        host.Run();
    }
}

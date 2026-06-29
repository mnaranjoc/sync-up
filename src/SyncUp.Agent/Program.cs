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

        string apiUrl = builder.Configuration["Api"] ?? throw new InvalidOperationException("The 'Api' configuration key is missing.");
        builder.Services.AddHttpClient<IFileWatcherService, FileWatcherService>(client =>
        {
            client.BaseAddress = new Uri(apiUrl);
        });

        var host = builder.Build();
        host.Run();
    }
}

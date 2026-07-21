using SyncUp.Agent.Application.Synchronization.Queue;
using SyncUp.Agent.Application.Synchronization.Services;
using SyncUp.Agent.Application.SyncUp;
using SyncUp.Agent.Application.SyncUp.Services;
using SyncUp.Agent.Application.Watcher.Services;
using SyncUp.Agent.Infrastructure.Api;

namespace SyncUp.Agent;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSingleton<IFileService, FileService>();
        builder.Services.AddSingleton<IFileWatcherService, FileWatcherService>();
        builder.Services.AddSingleton<ISynchronizationQueue, SynchronizationQueue>();
        builder.Services.AddTransient<ISyncUpService, SyncUpService>();
        builder.Services.AddTransient<ISyncUpApiClient, SyncUpApiClient>();
        builder.Services.AddHostedService<WatcherTask>();
        builder.Services.AddHostedService<SyncUpTask>();

        string apiUrl = builder.Configuration["Api"] ?? throw new InvalidOperationException("The 'Api' configuration key is missing.");
        builder.Services.AddHttpClient<ISyncUpApiClient, SyncUpApiClient>(client => { client.BaseAddress = new Uri(apiUrl); });

        var host = builder.Build();
        host.Run();
    }
}

using SyncUp.Agent.Application.SyncUp;
using SyncUp.Agent.Application.Watcher.Services.FileWatcherService;
using SyncUp.Agent.Application.SyncUp.Services.SyncUpService;
using SyncUp.Agent.Application.SyncUp.Services.AgentFilesService;
using SyncUp.Agent.Infrastructure.Api;

namespace SyncUp.Agent;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSingleton<IAgentFilesService, AgentFileService>();
        builder.Services.AddSingleton<IFileWatcherService, FileWatcherService>();
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

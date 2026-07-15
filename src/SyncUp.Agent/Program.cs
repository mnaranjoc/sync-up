using SyncUp.Agent.Services;
using SyncUp.Agent.Services.FileWatcherService;
using SyncUp.Agent.Services.AgentFilesService;
using SyncUp.Agent.Services.SyncUpService;

namespace SyncUp.Agent;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSingleton<IAgentFilesService, AgentFileService>();
        builder.Services.AddSingleton<IFileWatcherService, FileWatcherService>();
        builder.Services.AddTransient<ISyncUpService, SyncUpService>();
        builder.Services.AddHostedService<WatcherTask>();
        builder.Services.AddHostedService<SyncTask>();

        string apiUrl = builder.Configuration["Api"] ?? throw new InvalidOperationException("The 'Api' configuration key is missing.");
        //builder.Services.AddHttpClient<IFileWatcherService, FileWatcherService>(client => { client.BaseAddress = new Uri(apiUrl); });
        builder.Services.AddHttpClient<ISyncUpService, SyncUpService>(client => { client.BaseAddress = new Uri(apiUrl); });

        var host = builder.Build();
        host.Run();
    }
}

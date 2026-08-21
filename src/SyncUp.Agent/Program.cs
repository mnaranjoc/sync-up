using SyncUp.Agent.Application.Synchronization;
using SyncUp.Agent.Application.Synchronization.Queue;
using SyncUp.Agent.Application.Synchronization.Services;
using SyncUp.Agent.Application.Synchronization.Services.Strategy;
using SyncUp.Agent.Application.SyncUp;
using SyncUp.Agent.Application.Watcher.Abstractions;
using SyncUp.Agent.Application.Watcher.Services;
using SyncUp.Agent.Infrastructure.Api;

namespace SyncUp.Agent;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        // Synchronization
        builder.Services.AddSingleton<ISynchronizationQueue, SynchronizationQueue>();

        // SyncUp
        builder.Services.AddHostedService<SynchronizationTask>();
        builder.Services.AddSingleton<ISynchronizationService, SynchronizationService>();
        builder.Services.AddTransient<ISynchronizationStrategy, UnknownSyncStrategy>();
        builder.Services.AddTransient<ISynchronizationStrategy, OutOfSyncStrategy>();
        builder.Services.AddTransient<ISynchronizationStrategy, InSyncStrategy>();

        // Watcher
        builder.Services.AddHostedService<WatcherTask>();
        builder.Services.AddSingleton<IFileWatcherService, FileWatcherService>();
        builder.Services.AddTransient<IFileSystemWatcherWrapper, FileSystemWatcherWrapper>();

        // Api client
        builder.Services.AddSingleton<IApiClient, ApiClient>();

        // Http client
        string apiUrl = builder.Configuration["Api"] ?? throw new InvalidOperationException("The 'Api' configuration key is missing.");
        builder.Services.AddHttpClient<IApiClient, ApiClient>(client => { client.BaseAddress = new Uri(apiUrl); });

        var host = builder.Build();
        host.Run();
    }
}

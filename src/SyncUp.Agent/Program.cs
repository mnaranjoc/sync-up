using SyncUp.Agent.Service;

namespace SyncUp.Agent;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<WatcherService>();

        var host = builder.Build();
        host.Run();
    }
}
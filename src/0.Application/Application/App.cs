using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace GGroupp.Platrom.Bot.ProactiveMessage.Send;

internal static class App
{
    public static async Task RunAsync(CancellationToken cancellationToken)
    {
        using var host = BuildHost();
        await host.RunAsync(cancellationToken);
    }

    private static IHost BuildHost()
        =>
        new HostBuilder()
        .ConfigureSocketsHttpHandlerProvider()
        .ConfigureMessageSendQueueProcesor()
        .ConfigureStandardLogging()
        .Build();
}
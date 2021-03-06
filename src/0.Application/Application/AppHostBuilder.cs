using System;
using GGroupp.Infra;
using GGroupp.Infra.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PrimeFuncPack;

namespace GGroupp.Platrom.Bot.ProactiveMessage.Send;

internal static class AppHostBuilder
{
    internal static IHostBuilder ConfigureMessageSendQueueProcessor(this IHostBuilder hostBuilder)
        =>
        IsServiceBusUsed() switch
        {
            true    => UseMessageSendQueue().ConfigureBusQueueProcessor(hostBuilder),
            _       => UseMessageSendQueue().ConfigureQueueProcessor(hostBuilder)
        };

    private static bool IsServiceBusUsed()
        =>
        new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build()
        .GetValue("Feature:IsServiceBusUsed", false);

    private static Dependency<IQueueItemHandler> UseMessageSendQueue()
        =>
        PrimaryHandler.UseStandardSocketsHttpHandler()
        .UseLogging(
            sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger("ProactiveMessageSend"))
        .UseCosmosApi(
            sp => sp.GetConfiguration().GetSection("CosmosApi").GetCosmosApiOption())
        .UseConversationGetApi()
        .With(
            Dependency.From(GetConfiguration).UseConversationContinueApi())
        .UseMessageSendLogic()
        .UseMessageSendQueue();

    private static IConfiguration GetConfiguration(this IServiceProvider serviceProvider)
        =>
        serviceProvider.GetRequiredService<IConfiguration>();

    private static CosmosApiOption GetCosmosApiOption(this IConfigurationSection section)
        =>
        new(
            baseAddress: new(section.GetValue<string>("BaseAddressUrl")),
            masterKey: section.GetValue<string>("MasterKey"),
            databaseId: section.GetValue<string>("DatabaseId"));
}

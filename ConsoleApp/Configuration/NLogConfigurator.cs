using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;

namespace ConsoleApp.Configuration;

internal static class NLogConfigurator
{
    public static IServiceCollection SetupNLog(this IServiceCollection serviceCollection)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("nlog.config"));

        using Stream stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded NLog config resource '{resourceName}' not found.");
        using StreamReader reader = new(stream);
        var configContent = reader.ReadToEnd();

        var nlogConfig = XmlLoggingConfiguration.CreateFromXmlString(configContent);

        return serviceCollection
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Warning);
                builder.AddNLog(nlogConfig, new NLogProviderOptions
                {
                    CaptureMessageProperties = true,
                    CaptureMessageTemplates = true
                });
            });
    }
}

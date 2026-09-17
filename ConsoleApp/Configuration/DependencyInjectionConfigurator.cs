using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Domain.DependencyInjection;

namespace ConsoleApp.Configuration;

internal static class DependencyInjectionConfigurator
{
    public static ServiceProvider Setup()
    {
        var serviceProvider = new ServiceCollection()
            .RegisterDomainClasses()
            .ConfigureHttpClients(GetAppVersion())
            .SetupNLog()
            .BuildServiceProvider();

        return serviceProvider;
    }

    private static string? GetAppVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
    }
}

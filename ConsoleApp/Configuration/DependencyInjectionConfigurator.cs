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

    internal static string GetAppVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version
            ?? throw new InvalidOperationException("Could not determine the application version from the executing assembly.");

        return version.ToString(3);
    }
}

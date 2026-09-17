using Microsoft.Extensions.DependencyInjection;

using Domain.DependencyInjection;

namespace ConsoleApp.Configuration;

internal static class DependencyInjectionConfigurator
{
    public static ServiceProvider Setup()
    {
        var serviceProvider = new ServiceCollection()
            .RegisterDomainClasses()
            .ConfigureHttpClients()
            .SetupNLog()
            .BuildServiceProvider();

        return serviceProvider;
    }
}

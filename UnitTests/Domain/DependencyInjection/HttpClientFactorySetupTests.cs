using Microsoft.Extensions.DependencyInjection;
using Domain.Clients;
using Domain.Constants;
using Domain.DependencyInjection;

namespace UnitTests.Domain.DependencyInjection;

public class HttpClientFactorySetupTests
{
    private static HttpClient CreateNamedClient(string? appVersion)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.ConfigureHttpClients(appVersion);
        var provider = services.BuildServiceProvider();

        var factory = provider.GetRequiredService<IHttpClientFactory>();
        return factory.CreateClient(nameof(IMoxfieldClient));
    }

    [Fact]
    public void ConfigureHttpClients_WithAppVersion_SetsVersionedUserAgent()
    {
        using var client = CreateNamedClient("1.2.3");

        Assert.Equal($"{HttpClientSettings.APP_NAME}/1.2.3", client.DefaultRequestHeaders.UserAgent.ToString());
    }

    [Fact]
    public void ConfigureHttpClients_WithoutAppVersion_SetsUnversionedUserAgent()
    {
        using var client = CreateNamedClient(null);

        Assert.Equal(HttpClientSettings.APP_NAME, client.DefaultRequestHeaders.UserAgent.ToString());
    }

    [Fact]
    public void ConfigureHttpClients_SetsConfiguredTimeout()
    {
        using var client = CreateNamedClient("1.2.3");

        Assert.Equal(TimeSpan.FromSeconds(HttpClientSettings.DEFAULT_TIMEOUT_SECONDS), client.Timeout);
    }

    [Fact]
    public void ConfigureHttpClients_SetsBaseAddressPerClient()
    {
        using var client = CreateNamedClient("1.2.3");

        Assert.Equal(HttpClientSettings.MOXFIELD_BASE_URL, client.BaseAddress!.AbsoluteUri);
    }
}

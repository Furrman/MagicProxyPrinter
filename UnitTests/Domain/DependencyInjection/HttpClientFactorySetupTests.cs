using Microsoft.Extensions.DependencyInjection;
using Domain.Clients;
using Domain.Constants;
using Domain.DependencyInjection;

namespace UnitTests.Domain.DependencyInjection;

public class HttpClientFactorySetupTests
{
    private static HttpClient CreateNamedClient(string? appVersion, string clientInterfaceName = nameof(IMoxfieldClient))
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.ConfigureHttpClients(appVersion);
        var provider = services.BuildServiceProvider();

        var factory = provider.GetRequiredService<IHttpClientFactory>();
        return factory.CreateClient(clientInterfaceName);
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

    [Theory]
    [InlineData(nameof(IArchidektClient))]
    [InlineData(nameof(IMoxfieldClient))]
    [InlineData(nameof(IEdhrecClient))]
    [InlineData(nameof(IGoldfishClient))]
    [InlineData(nameof(ICubeCobraClient))]
    [InlineData(nameof(IScryfallClient))]
    public void ConfigureHttpClients_SetsUserAgent_OnEveryRegisteredClient(string clientInterfaceName)
    {
        using var client = CreateNamedClient("1.2.3", clientInterfaceName);

        Assert.Equal($"{HttpClientSettings.APP_NAME}/1.2.3", client.DefaultRequestHeaders.UserAgent.ToString());
    }
}

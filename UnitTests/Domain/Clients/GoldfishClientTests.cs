using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Domain.Clients;
using Domain.Constants;

namespace UnitTests.Domain.Clients;

public class GoldfishClientTests
{
    private static GoldfishClient CreateClient(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(HttpClientSettings.GOLDFISH_BASE_URL) };
        return new GoldfishClient(httpClient, Mock.Of<ILogger<GoldfishClient>>());
    }

    [Fact]
    public async Task GetCardsInHtml_RequestsExpectedUrl()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, new StringContent("<html></html>"));
        var client = CreateClient(handler);

        await client.GetCardsInHtml("deck/1234");

        Assert.Equal($"{HttpClientSettings.GOLDFISH_BASE_URL}deck/1234", handler.LastRequest!.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task GetCardsInHtml_SuccessResponse_ReturnsHtmlContent()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, new StringContent("<html>deck</html>"));
        var client = CreateClient(handler);

        var result = await client.GetCardsInHtml("deck/1234");

        Assert.Equal("<html>deck</html>", result);
    }

    [Fact]
    public async Task GetCardsInHtml_NotFoundResponse_ReturnsNull()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NotFound);
        var client = CreateClient(handler);

        var result = await client.GetCardsInHtml("deck/1234");

        Assert.Null(result);
    }
}

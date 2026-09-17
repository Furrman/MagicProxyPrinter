using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Domain.Clients;
using Domain.Constants;

namespace UnitTests.Domain.Clients;

public class EdhrecClientTests
{
    private static EdhrecClient CreateClient(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(HttpClientSettings.EDHREC_BASE_URL) };
        return new EdhrecClient(httpClient, Mock.Of<ILogger<EdhrecClient>>());
    }

    [Fact]
    public async Task GetCardsInHtml_RequestsExpectedUrl()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, new StringContent("<html></html>"));
        var client = CreateClient(handler);

        await client.GetCardsInHtml("commanders/atraxa");

        Assert.Equal($"{HttpClientSettings.EDHREC_BASE_URL}commanders/atraxa", handler.LastRequest!.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task GetCardsInHtml_SuccessResponse_ReturnsHtmlContent()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, new StringContent("<html>deck</html>"));
        var client = CreateClient(handler);

        var result = await client.GetCardsInHtml("commanders/atraxa");

        Assert.Equal("<html>deck</html>", result);
    }

    [Fact]
    public async Task GetCardsInHtml_NotFoundResponse_ReturnsNull()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NotFound);
        var client = CreateClient(handler);

        var result = await client.GetCardsInHtml("commanders/atraxa");

        Assert.Null(result);
    }
}

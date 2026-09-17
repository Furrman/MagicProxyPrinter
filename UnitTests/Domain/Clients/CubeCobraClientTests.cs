using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Domain.Clients;

namespace UnitTests.Domain.Clients;

public class CubeCobraClientTests
{
    private const string BaseAddress = "https://www.cubecobra.com/";

    private static CubeCobraClient CreateClient(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress) };
        return new CubeCobraClient(httpClient, Mock.Of<ILogger<CubeCobraClient>>());
    }

    [Fact]
    public async Task GetCardsInHtml_RequestsExpectedUrl()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, new StringContent("<html></html>"));
        var client = CreateClient(handler);

        await client.GetCardsInHtml("my-cube");

        Assert.Equal("https://www.cubecobra.com/cube/list/my-cube", handler.LastRequest!.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task GetCardsInHtml_SuccessResponse_ReturnsHtmlContent()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, new StringContent("<html>deck</html>"));
        var client = CreateClient(handler);

        var result = await client.GetCardsInHtml("my-cube");

        Assert.Equal("<html>deck</html>", result);
    }

    [Fact]
    public async Task GetCardsInHtml_NotFoundResponse_ReturnsNull()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NotFound);
        var client = CreateClient(handler);

        var result = await client.GetCardsInHtml("my-cube");

        Assert.Null(result);
    }
}

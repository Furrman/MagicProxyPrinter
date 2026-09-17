using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Domain.Clients;
using Domain.Constants;
using Domain.Models.DTO.Moxfield;

namespace UnitTests.Domain.Clients;

public class MoxfieldClientTests
{
    private static MoxfieldClient CreateClient(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(HttpClientSettings.MOXFIELD_BASE_URL) };
        return new MoxfieldClient(httpClient, Mock.Of<ILogger<MoxfieldClient>>());
    }

    [Fact]
    public async Task GetDeck_RequestsExpectedUrl()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NotFound);
        var client = CreateClient(handler);

        await client.GetDeck("abc123");

        Assert.Equal($"{HttpClientSettings.MOXFIELD_BASE_URL}decks/all/abc123", handler.LastRequest!.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task GetDeck_SuccessResponse_ReturnsParsedDeck()
    {
        var deckDto = new DeckDTO("Test Deck");
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, JsonContent.Create(deckDto));
        var client = CreateClient(handler);

        var result = await client.GetDeck("abc123");

        Assert.NotNull(result);
        Assert.Equal("Test Deck", result.Name);
    }

    [Fact]
    public async Task GetDeck_NotFoundResponse_ReturnsNull()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NotFound);
        var client = CreateClient(handler);

        var result = await client.GetDeck("abc123");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetDeck_MalformedJsonResponse_ReturnsNull()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, new StringContent("not json"));
        var client = CreateClient(handler);

        var result = await client.GetDeck("abc123");

        Assert.Null(result);
    }
}

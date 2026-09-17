using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Domain.Clients;
using Domain.Constants;
using Domain.Models.DTO.Archidekt;

namespace UnitTests.Domain.Clients;

public class ArchidektClientTests
{
    private static ArchidektClient CreateClient(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(HttpClientSettings.ARCHIDEKT_BASE_URL) };
        return new ArchidektClient(httpClient, Mock.Of<ILogger<ArchidektClient>>());
    }

    [Fact]
    public async Task GetDeck_RequestsExpectedUrl()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NotFound);
        var client = CreateClient(handler);

        await client.GetDeck(123456);

        Assert.Equal($"{HttpClientSettings.ARCHIDEKT_BASE_URL}decks/123456/", handler.LastRequest!.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task GetDeck_SuccessResponse_ReturnsParsedDeck()
    {
        var deckDto = new DeckDTO("Test Deck", [new DeckCardDTO(new CardDTO(new OracleCardDTO("Card 1"), new EditionDTO()), 2)]);
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, JsonContent.Create(deckDto));
        var client = CreateClient(handler);

        var result = await client.GetDeck(123456);

        Assert.NotNull(result);
        Assert.Equal("Test Deck", result.Name);
        Assert.Single(result.Cards!);
    }

    [Fact]
    public async Task GetDeck_NotFoundResponse_ReturnsNull()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NotFound);
        var client = CreateClient(handler);

        var result = await client.GetDeck(123456);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetDeck_MalformedJsonResponse_ReturnsNull()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, new StringContent("not json"));
        var client = CreateClient(handler);

        var result = await client.GetDeck(123456);

        Assert.Null(result);
    }
}

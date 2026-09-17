using System.Net;

using Microsoft.Extensions.Logging;

using Moq;

using Domain.Clients;

namespace UnitTests.Domain.Clients;

public class ScryfallClientTests
{
    private const string BaseAddress = "https://api.scryfall.com/";

    private static ScryfallClient CreateClient(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress) };
        return new ScryfallClient(httpClient, Mock.Of<ILogger<ScryfallClient>>());
    }

    [Fact]
    public async Task FindCard_WithLanguageCode_BuildsUrlWithSlashSeparator()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NotFound);
        var client = CreateClient(handler);

        await client.FindCard("Lightning Bolt", "LEA", "161", "en");

        Assert.Equal("https://api.scryfall.com/cards/LEA/161/en", handler.LastRequest!.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task FindCard_WithoutLanguageCode_BuildsUrlWithoutTrailingSegment()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NotFound);
        var client = CreateClient(handler);

        await client.FindCard("Lightning Bolt", "LEA", "161");

        Assert.Equal("https://api.scryfall.com/cards/LEA/161", handler.LastRequest!.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task SearchCard_BuildsUrlWithoutStrayDollarSign()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NotFound);
        var client = CreateClient(handler);

        await client.SearchCard("Lightning Bolt", includeExtras: false, includeMultilingual: false);

        Assert.Equal("https://api.scryfall.com/cards/search?q=Lightning%20Bolt", handler.LastRequest!.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task SearchCard_EscapesSpecialCharactersInCardName()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NotFound);
        var client = CreateClient(handler);

        await client.SearchCard("Fire // Ice", includeExtras: false, includeMultilingual: false);

        Assert.Equal("https://api.scryfall.com/cards/search?q=Fire%20%2F%2F%20Ice", handler.LastRequest!.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task SearchCard_WithExtrasAndMultilingual_AppendsBothQueryFlags()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NotFound);
        var client = CreateClient(handler);

        await client.SearchCard("Lightning Bolt", includeExtras: true, includeMultilingual: true);

        var requestUrl = handler.LastRequest!.RequestUri!.AbsoluteUri;
        Assert.Contains("unique=prints&include_extras=true&include_variations=true", requestUrl);
        Assert.Contains("include_multilingual=true", requestUrl);
    }
}

using Microsoft.Extensions.Logging;
using Moq;
using Domain.Factories;
using Domain.Models.DTO;
using Domain.Services;
using Domain.Strategies;

namespace UnitTests.Domain.Strategies;

public class DeckRetrieveStrategyTests
{
    private readonly Mock<IServiceFactory> _serviceFactoryMock;
    private readonly DeckRetrieveStrategy _strategy;

    public DeckRetrieveStrategyTests()
    {
        _serviceFactoryMock = new Mock<IServiceFactory>();
        _strategy = new DeckRetrieveStrategy(_serviceFactoryMock.Object, Mock.Of<ILogger<DeckRetrieveStrategy>>());
    }

    [Fact]
    public async Task GetDeck_NoMatchingService_ReturnsNull()
    {
        _serviceFactoryMock.Setup(f => f.GetDeckBuildService(It.IsAny<string>())).Returns((IDeckBuildService?)null);

        var result = await _strategy.GetDeck("https://unsupported-site.com/deck/1");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetDeck_ForNonEdhrecService_DelegatesToService()
    {
        var deckUrl = "https://moxfield.com/decks/abc123";
        var expectedDeck = new DeckDetailsDTO { Name = "Test Deck" };
        var serviceMock = new Mock<IDeckBuildService>();
        serviceMock.Setup(s => s.RetrieveDeckFromWeb(deckUrl)).ReturnsAsync(expectedDeck);
        _serviceFactoryMock.Setup(f => f.GetDeckBuildService(deckUrl)).Returns(serviceMock.Object);

        var result = await _strategy.GetDeck(deckUrl);

        Assert.Same(expectedDeck, result);
    }

    [Fact]
    public async Task GetDeck_ForEdhrecService_WithOriginalLinkAndMatchingService_UsesOriginalService()
    {
        var deckUrl = "https://edhrec.com/deckpreview/abc123";
        var originalDeckUrl = "https://moxfield.com/decks/abc123";
        var expectedDeck = new DeckDetailsDTO { Name = "Original Deck" };

        var edhrecServiceMock = new Mock<IEdhrecService>();
        edhrecServiceMock.Setup(s => s.GetOriginalDeckLink(deckUrl)).ReturnsAsync((originalDeckUrl, "<html/>"));

        var originalServiceMock = new Mock<IDeckBuildService>();
        originalServiceMock.Setup(s => s.RetrieveDeckFromWeb(originalDeckUrl)).ReturnsAsync(expectedDeck);

        _serviceFactoryMock.Setup(f => f.GetDeckBuildService(deckUrl)).Returns(edhrecServiceMock.Object);
        _serviceFactoryMock.Setup(f => f.GetDeckBuildService(originalDeckUrl)).Returns(originalServiceMock.Object);

        var result = await _strategy.GetDeck(deckUrl);

        Assert.Same(expectedDeck, result);
        edhrecServiceMock.Verify(s => s.ScrapDeckFromHtml(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetDeck_ForEdhrecService_WithOriginalLinkButNoMatchingService_FallsBackToScrapedHtml()
    {
        var deckUrl = "https://edhrec.com/deckpreview/abc123";
        var originalDeckUrl = "https://unsupported-site.com/decks/abc123";
        var scrapedDeck = new DeckDetailsDTO { Name = "Scraped Deck" };

        var edhrecServiceMock = new Mock<IEdhrecService>();
        edhrecServiceMock.Setup(s => s.GetOriginalDeckLink(deckUrl)).ReturnsAsync((originalDeckUrl, "<html/>"));
        edhrecServiceMock.Setup(s => s.ScrapDeckFromHtml("<html/>")).Returns(scrapedDeck);

        _serviceFactoryMock.Setup(f => f.GetDeckBuildService(deckUrl)).Returns(edhrecServiceMock.Object);
        _serviceFactoryMock.Setup(f => f.GetDeckBuildService(originalDeckUrl)).Returns((IDeckBuildService?)null);

        var result = await _strategy.GetDeck(deckUrl);

        Assert.Same(scrapedDeck, result);
    }

    [Fact]
    public async Task GetDeck_ForEdhrecService_WithoutOriginalLink_ScrapesHtmlDirectly()
    {
        var deckUrl = "https://edhrec.com/deckpreview/abc123";
        var scrapedDeck = new DeckDetailsDTO { Name = "Scraped Deck" };

        var edhrecServiceMock = new Mock<IEdhrecService>();
        edhrecServiceMock.Setup(s => s.GetOriginalDeckLink(deckUrl)).ReturnsAsync(((string?)null, "<html/>"));
        edhrecServiceMock.Setup(s => s.ScrapDeckFromHtml("<html/>")).Returns(scrapedDeck);

        _serviceFactoryMock.Setup(f => f.GetDeckBuildService(deckUrl)).Returns(edhrecServiceMock.Object);

        var result = await _strategy.GetDeck(deckUrl);

        Assert.Same(scrapedDeck, result);
    }

    [Fact]
    public async Task GetDeck_ForEdhrecService_WithoutOriginalLinkOrHtml_ReturnsNull()
    {
        var deckUrl = "https://edhrec.com/deckpreview/abc123";

        var edhrecServiceMock = new Mock<IEdhrecService>();
        edhrecServiceMock.Setup(s => s.GetOriginalDeckLink(deckUrl)).ReturnsAsync(((string?)null, (string?)null));

        _serviceFactoryMock.Setup(f => f.GetDeckBuildService(deckUrl)).Returns(edhrecServiceMock.Object);

        var result = await _strategy.GetDeck(deckUrl);

        Assert.Null(result);
        edhrecServiceMock.Verify(s => s.ScrapDeckFromHtml(It.IsAny<string>()), Times.Never);
    }
}

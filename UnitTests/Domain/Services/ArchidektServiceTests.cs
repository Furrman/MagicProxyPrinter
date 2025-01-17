using Microsoft.Extensions.Logging;

using Moq;

using Domain.Clients;
using Domain.Services;
using Domain.Models.DTO.Archidekt;

namespace UnitTests.Domain.Services;

public class ArchidektServiceTests
{
    private readonly Mock<IArchidektClient> _archidektClientMock;
    private readonly Mock<ILogger<ArchidektService>> _loggerMock;
    private readonly ArchidektService _service;

    public ArchidektServiceTests()
    {
        _archidektClientMock = new Mock<IArchidektClient>();
        _loggerMock = new Mock<ILogger<ArchidektService>>();
        _service = new ArchidektService(_archidektClientMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("archidekt.com/api/decks/123/", 123)]
    [InlineData("www.archidekt.com/decks/456/", 456)]
    [InlineData("https://archidekt.com/api/decks/123/", 123)]
    [InlineData("https://archidekt.com/decks/456/", 456)]
    public void TryExtractDeckIdFromUrl_ValidUrl_ReturnsTrueAndExtractedDeckId(string url, int expectedDeckId)
    {
        // Act
        bool result = _service.TryExtractDeckIdFromUrl(url, out int deckId);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedDeckId, deckId);
    }

    [Theory]
    [InlineData("https://archidekt.com/api/decks/abc/")]
    [InlineData("https://archidekt.com/decks/xyz/")]
    [InlineData("https://archidekt.com/")]
    public void TryExtractDeckIdFromUrl_InvalidUrl_ReturnsFalse(string url)
    {
        // Act
        bool result = _service.TryExtractDeckIdFromUrl(url, out int deckId);

        // Assert
        Assert.False(result);
        Assert.Equal(0, deckId);
    }

    [Fact]
    public async Task RetrieveDeckFromWeb_WithValidDeckId_ReturnsDeckDetails()
    {
        // Arrange
        string deckUrl = "https://archidekt.com/decks/123456/test";
        var deckDto = new DeckDTO("Test Deck",
        [
            new DeckCardDTO(new CardDTO(new OracleCardDTO("Card 1"), new EditionDTO()), 2),
            new DeckCardDTO(new CardDTO(new OracleCardDTO("Card 2"), new EditionDTO()), 3)
        ]);

        _archidektClientMock.Setup(mock => mock.GetDeck(It.IsAny<int>())).ReturnsAsync(deckDto);

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Deck", result!.Name);
        Assert.Equal(2, result.Cards.Count);
        Assert.Equal("Card 1", result.Cards[0].Name);
        Assert.Equal(2, result.Cards[0].Quantity);
        Assert.Equal("Card 2", result.Cards[1].Name);
        Assert.Equal(3, result.Cards[1].Quantity);
    }

    [Fact]
    public async Task RetrieveDeckFromWeb_WithInvalidDeckId_ReturnsNull()
    {
        // Arrange
        string deckUrl = "https://archidekt.com/decks/123456/test";

        _archidektClientMock.Setup(mock => mock.GetDeck(It.IsAny<int>())).ReturnsAsync((DeckDTO?)null);

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RetrieveDeckFromWeb_WithEmptyName_ShouldBeIgnored()
    {
        // Arrange
        string deckUrl = "https://archidekt.com/decks/123456/test";
        var deckDto = new DeckDTO("", [new DeckCardDTO(new CardDTO(new OracleCardDTO(string.Empty), null), 1)]);

        _archidektClientMock.Setup(mock => mock.GetDeck(It.IsAny<int>())).ReturnsAsync(deckDto);

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result!.Cards);
    }

    [Fact]
    public async Task RetrieveDeckFromWeb_WithQuantity0_ShouldBeIgnored()
    {
        // Arrange
        string deckUrl = "https://archidekt.com/decks/123456/test";
        var deckDto = new DeckDTO("", [new DeckCardDTO(new CardDTO(new OracleCardDTO("Test"), null), 0)]);

        _archidektClientMock.Setup(mock => mock.GetDeck(It.IsAny<int>())).ReturnsAsync(deckDto);

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result!.Cards);
    }
    
    [Fact]
    public async Task RetrieveDeckFromWeb_WithArtSeriesTextInLayout_ShouldBeMarkedAsArt()
    {
        // Arrange
        string deckUrl = "https://archidekt.com/decks/123456/test";
        var deckDto = new DeckDTO("", [new DeckCardDTO(new CardDTO(new OracleCardDTO("Test", "art_series"), null), 1)]);

        _archidektClientMock.Setup(mock => mock.GetDeck(It.IsAny<int>())).ReturnsAsync(deckDto);

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result!.Cards);
        Assert.True(result.Cards[0].Art);
    }

    [Fact]
    public async Task RetrieveDeckFromWeb_WithEtchedInCardModifier_ShouldBeMarkedAsEtched()
    {
        // Arrange
        string deckUrl = "https://archidekt.com/decks/123456/test";
        var deckDto = new DeckDTO("", [new DeckCardDTO(new CardDTO(new OracleCardDTO("Test"), null), 1, "Etched")]);

        _archidektClientMock.Setup(mock => mock.GetDeck(It.IsAny<int>())).ReturnsAsync(deckDto);

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result!.Cards);
        Assert.True(result.Cards[0].Etched);
    }

    [Fact]
    public async Task RetrieveDeckFromWeb_WithFoilInCardModifier_ShouldBeMarkedAsFoil()
    {
        // Arrange
        string deckUrl = "https://archidekt.com/decks/123456/test";
        var deckDto = new DeckDTO("", [new DeckCardDTO(new CardDTO(new OracleCardDTO("Test"), null), 1, "Foil")]);

        _archidektClientMock.Setup(mock => mock.GetDeck(It.IsAny<int>())).ReturnsAsync(deckDto);

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result!.Cards);
        Assert.True(result.Cards[0].Foil);
    }
}
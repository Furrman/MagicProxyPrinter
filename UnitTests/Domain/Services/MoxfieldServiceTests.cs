using Microsoft.Extensions.Logging;

using Moq;

using Domain.Clients;
using Domain.Models.DTO.Moxfield;
using Domain.Services;

namespace UnitTests.Domain.Services;

public class MoxfieldServiceTests
{
    private readonly Mock<IMoxfieldClient> _moxfieldClientMock;
    private readonly Mock<ILogger<MoxfieldService>> _loggerMock;
    private readonly MoxfieldService _service;

    public MoxfieldServiceTests()
    {
        _moxfieldClientMock = new Mock<IMoxfieldClient>();
        _loggerMock = new Mock<ILogger<MoxfieldService>>();
        _service = new MoxfieldService(_moxfieldClientMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("moxfield.com/decks/123fdgd/", "123fdgd")]
    [InlineData("www.moxfield.com/decks/123fdgd/", "123fdgd")]
    [InlineData("https://moxfield.com/decks/123fdgd/", "123fdgd")]
    [InlineData("https://www.moxfield.com/decks/123fdgd/", "123fdgd")]
    public void TryExtractDeckIdFromUrl_ValidUrl_ReturnsTrueAndExtractedRelatedPath(string url, string expectedId)
    {
        // Act
        bool result = _service.TryExtractDeckIdFromUrl(url, out string deckId);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedId, deckId);
    }

    [Theory]
    [InlineData("https://moxfield.com/decks")]
    [InlineData("https://moxfield.com/decks/")]
    [InlineData("https://moxfield.com/")]
    public void TryExtractDeckIdFromUrl_InvalidUrl_ReturnsFalse(string url)
    {
        // Act
        bool result = _service.TryExtractDeckIdFromUrl(url, out string relatedPath);

        // Assert
        Assert.False(result);
        Assert.Equal(string.Empty, relatedPath);
    }

    [Fact]
    public async Task RetrieveDeckFromWeb_WithValidDeckId_ReturnsDeckDetails()
    {
        // Arrange
        string deckUrl = "https://moxfield.com/decks/123fdgd23456";
        _moxfieldClientMock.Setup(x => x.GetDeck(It.IsAny<string>())).ReturnsAsync(new DeckDTO());

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        Assert.NotNull(result);
    }
    
    [Fact]
    public async Task RetrieveDeckFromWeb_WithValidDeckId_SetDeckName()
    {
        // Arrange
        string deckUrl = "https://moxfield.com/decks/123fdgd23456";
        _moxfieldClientMock.Setup(x => x.GetDeck(It.IsAny<string>())).ReturnsAsync(new DeckDTO 
        { 
            Name = "Test Deck"
        });

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Deck", result!.Name);
    }

    [Fact]
    public async Task RetrieveDeckFromWeb_WithValidDeckId_SetCardDetails()
    {
        // Arrange
        string deckUrl = "https://moxfield.com/decks/123fdgd23456";
        var cardId = Guid.NewGuid();
        _moxfieldClientMock.Setup(x => x.GetDeck(It.IsAny<string>())).ReturnsAsync(new DeckDTO 
        { 
            Boards = new Dictionary<string, BoardDetailsDTO> 
            { 
                { 
                    "Mainboard",
                    new BoardDetailsDTO 
                    { 
                        Cards = new Dictionary<string, CardDTO> 
                        { 
                            { 
                                "ID", 
                                new CardDTO(
                                    2, 
                                    new CardDetailsDTO("ID", cardId, "UNIQUE_ID", "Test Card 1")
                                )
                            } 
                        } 
                    }
                } 
            } 
        });

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result!.Cards[0]);
        Assert.Equal(cardId, result.Cards[0].Id);
        Assert.Equal("Test Card 1", result.Cards[0].Name);
        Assert.Equal(2, result.Cards[0].Quantity);
    }
}
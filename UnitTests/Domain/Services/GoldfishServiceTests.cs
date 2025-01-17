using Microsoft.Extensions.Logging;

using Moq;

using Domain.Clients;
using Domain.Models.DTO.Moxfield;
using Domain.Services;

namespace UnitTests.Domain.Services;

public class GoldfishServiceTests
{
    private readonly Mock<IGoldfishClient> _goldfishClientMock;
    private readonly Mock<ILogger<GoldfishService>> _loggerMock;
    private readonly GoldfishService _service;

    public GoldfishServiceTests()
    {
        _goldfishClientMock = new Mock<IGoldfishClient>();
        _loggerMock = new Mock<ILogger<GoldfishService>>();
        _service = new GoldfishService(_goldfishClientMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("mtggoldfish.com/deck/123fdgd", "deck/123fdgd")]
    [InlineData("www.mtggoldfish.com/deck/123fdgd", "deck/123fdgd")]
    [InlineData("https://mtggoldfish.com/deck/123fdgd", "deck/123fdgd")]
    [InlineData("https://www.mtggoldfish.com/deck/123fdgd", "deck/123fdgd")]
    public void TryExtractRelativePath_ValidUrl_ReturnsTrueAndExtractedRelatedPath(string url, string expectedPath)
    {
        // Act
        bool result = _service.TryExtractRelativePath(url, out string relativePath);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedPath, relativePath);
    }

    [Theory]
    [InlineData("https://mtggoldfish.com/deck")]
    [InlineData("https://mtggoldfish.com/deck/")]
    [InlineData("https://mtggoldfish.com/deck/123fdgd/123fdgd")]
    [InlineData("https://mtggoldfish.com/")]
    public void TryExtractRelativePath_InvalidUrl_ReturnsFalse(string url)
    {
        // Act
        bool result = _service.TryExtractRelativePath(url, out string relatedPath);

        // Assert
        Assert.False(result);
        Assert.Equal(string.Empty, relatedPath);
    }

    [Fact]
    public async Task RetrieveDeckFromWeb_WithValidDeckId_ReturnsDeckDetails()
    {
        // Arrange
        string deckUrl = "https://mtggoldfish.com/deck/123fdgd23456";
        _goldfishClientMock.Setup(x => x.GetCardsInHtml(It.IsAny<string>()))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        Assert.NotNull(result);
    }
    
    [Fact]
    public async Task RetrieveDeckFromWeb_WithValidDeckId_SetDeckName()
    {
        // Arrange
        string deckUrl = "https://mtggoldfish.com/deck/123fdgd23456";
        _goldfishClientMock.Setup(x => x.GetCardsInHtml(It.IsAny<string>()))
            .ReturnsAsync("""
                          <html>
                          <body>
                              <h1 class="title">My First Deck</h1>
                          </body>
                          </html>
                          """);

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("My First Deck", result!.Name);
    }

    [Fact]
    public async Task RetrieveDeckFromWeb_WithValidDeckId_SetCardDetails()
    {
        // Arrange
        string deckUrl = "https://mtggoldfish.com/deck/123fdgd23456";
        _goldfishClientMock.Setup(x => x.GetCardsInHtml(It.IsAny<string>()))
            .ReturnsAsync("""
                          <html>
                          <body>
                              <h1 class="title">Ultimate Magic Deck</h1>
                              <input id="deck_input_deck" type="hidden" value="4 Omniscience" />
                          </body>
                          </html>
                          """);

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result!.Cards[0]);
        Assert.Null(result.Cards[0].Id);
        Assert.Equal("Omniscience", result.Cards[0].Name);
        Assert.Equal(4, result.Cards[0].Quantity);
    }
}

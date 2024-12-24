using Microsoft.Extensions.Logging;

using FluentAssertions;
using Moq;

using Domain.Clients;
using Domain.Models.DTO.Moxfield;

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
    [InlineData("https://moxfield.com/decks/123fdgd/", "123fdgd")]
    [InlineData("https://www.moxfield.com/decks/123fdgd/", "123fdgd")]
    public void TryExtractDeckIdFromUrl_ValidUrl_ReturnsTrueAndExtractedDeckId(string url, string expectedDeckId)
    {
        // Act
        bool result = _service.TryExtractDeckIdFromUrl(url, out string deckId);

        // Assert
        result.Should().BeTrue();
        deckId.Should().Be(expectedDeckId);
    }

    [Theory]
    [InlineData("https://moxfield.com/decks")]
    [InlineData("https://moxfield.com/decks/")]
    [InlineData("https://moxfield.com/")]
    public void TryExtractDeckIdFromUrl_InvalidUrl_ReturnsFalse(string url)
    {
        // Act
        bool result = _service.TryExtractDeckIdFromUrl(url, out string deckId);

        // Assert
        result.Should().BeFalse();
        deckId.Should().Be(string.Empty);
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
        result.Should().NotBeNull();
    }
}
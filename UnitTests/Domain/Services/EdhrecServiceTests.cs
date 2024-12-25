using Microsoft.Extensions.Logging;

using FluentAssertions;
using Moq;

using Domain.Clients;
using Domain.Models.DTO.Moxfield;
using Domain.Services;

namespace UnitTests.Domain.Services;

public class EdhrecServiceTests
{
    private readonly Mock<IEdhrecClient> _edhrecClientMock;
    private readonly Mock<ILogger<EdhrecService>> _loggerMock;
    private readonly EdhrecService _service;

    public EdhrecServiceTests()
    {
        _edhrecClientMock = new Mock<IEdhrecClient>();
        _loggerMock = new Mock<ILogger<EdhrecService>>();
        _service = new EdhrecService(_edhrecClientMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("https://www.edhrec.com/deckpreview/7VNuM_Ce5b3JbQrhfTsObA", "7VNuM_Ce5b3JbQrhfTsObA")]
    [InlineData("https://edhrec.com/deckpreview/7VNuM_Ce5b3JbQrhfTsObA", "7VNuM_Ce5b3JbQrhfTsObA")]
    public void TryExtractDeckIdFromUrl_ValidUrl_ReturnsTrueAndExtractedDeckId(string url, string expectedDeckId)
    {
        // Act
        bool result = _service.TryExtractDeckIdFromUrl(url, out string deckId);

        // Assert
        result.Should().BeTrue();
        deckId.Should().Be(expectedDeckId);
    }

    [Theory]
    [InlineData("https://edhrec.com/deckpreview")]
    [InlineData("https://edhrec.com/deckpreview/")]
    [InlineData("https://edhrec.com/")]
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
        string deckUrl = "https://edhrec.com/deckpreview/7VNuM_Ce5b3JbQrhfTsObA";
        _edhrecClientMock.Setup(x => x.GetDeck(It.IsAny<string>())).ReturnsAsync(string.Empty);

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        result.Should().NotBeNull();
    }
}
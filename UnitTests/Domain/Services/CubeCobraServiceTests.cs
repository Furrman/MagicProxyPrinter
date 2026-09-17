using Microsoft.Extensions.Logging;
using Moq;
using Domain.Clients;
using Domain.Services;

namespace UnitTests.Domain.Services;

public class CubeCobraServiceTests
{
    private readonly Mock<ICubeCobraClient> _cubeCobraClientMock;
    private readonly Mock<ILogger<CubeCobraService>> _loggerMock;
    private readonly CubeCobraService _service;

    public CubeCobraServiceTests()
    {
        _cubeCobraClientMock = new Mock<ICubeCobraClient>();
        _loggerMock = new Mock<ILogger<CubeCobraService>>();
        _service = new CubeCobraService(_cubeCobraClientMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("cubecobra.com/cube/list/5b73c9bb-4928-4d6a-9580-30d5a718d925", "5b73c9bb-4928-4d6a-9580-30d5a718d925")]
    [InlineData("www.cubecobra.com/cube/list/5b73c9bb-4928-4d6a-9580-30d5a718d925", "5b73c9bb-4928-4d6a-9580-30d5a718d925")]
    [InlineData("https://cubecobra.com/cube/list/5b73c9bb-4928-4d6a-9580-30d5a718d925", "5b73c9bb-4928-4d6a-9580-30d5a718d925")]
    [InlineData("https://www.cubecobra.com/cube/list/5b73c9bb-4928-4d6a-9580-30d5a718d925", "5b73c9bb-4928-4d6a-9580-30d5a718d925")]
    public void TryExtractRelativePath_ValidUrl_ReturnsTrueAndExtractedRelatedPath(string url, string expectedPath)
    {
        // Act
        bool result = _service.TryExtractRelativePath(url, out Guid relativePath);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedPath, relativePath.ToString());
    }

    [Theory]
    [InlineData("https://cubecobra.com/5b73c9bb-4928-4d6a-9580-30d5a718d925")]
    [InlineData("https://cubecobra.com/cube/5b73c9bb-4928-4d6a-9580-30d5a718d925")]
    [InlineData("https://cubecobra.com/cube/list")]
    [InlineData("https://cubecobra.com/cube/list/")]
    [InlineData("https://cubecobra.com/cube/list/notGuid")]
    [InlineData("https://cubecobra.com/cube/list/5b73c9bb-4928-4d6a-9580-30d5a718d925/5b73c9bb-4928-4d6a-9580-30d5a718d925")]
    public void TryExtractRelativePath_InvalidUrl_ReturnsFalse(string url)
    {
        // Act
        bool result = _service.TryExtractRelativePath(url, out Guid deckId);

        // Assert
        Assert.False(result);
        Assert.Equal(Guid.Empty, deckId);
    }

    [Fact]
    public async Task RetrieveDeckFromWeb_WithInvalidUrl_ReturnsNullWithoutCallingClient()
    {
        // Arrange
        string deckUrl = "https://cubecobra.com/cube/list/notGuid";

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        Assert.Null(result);
        _cubeCobraClientMock.Verify(x => x.GetCardsInHtml(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RetrieveDeckFromWeb_WithValidDeckId_ReturnsDeckDetails()
    {
        // Arrange
        string deckUrl = "https://cubecobra.com/cube/list/5b73c9bb-4928-4d6a-9580-30d5a718d925";
        _cubeCobraClientMock.Setup(x => x.GetCardsInHtml("5b73c9bb-4928-4d6a-9580-30d5a718d925"))
            .ReturnsAsync("""
                          <html><body><script>
                          window.reactProps = {"cube":{"name":"My Cube"},"cards":{"mainboard":[{"name":"Lightning Bolt","cardID":"5b73c9bb-4928-4d6a-9580-30d5a718d925","details":{"set":"LEA","collector_number":"161"},"finish":"Non-foil"}]}};
                          </script></body></html>
                          """);

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("My Cube", result.Name);
        Assert.Single(result.Cards);
        Assert.Equal("Lightning Bolt", result.Cards[0].Name);
        Assert.Equal("LEA", result.Cards[0].ExpansionCode);
        Assert.Equal("161", result.Cards[0].CollectorNumber);
    }

    [Fact]
    public async Task RetrieveDeckFromWeb_WithoutExpectedScriptContent_ReturnsNull()
    {
        // Arrange
        string deckUrl = "https://cubecobra.com/cube/list/5b73c9bb-4928-4d6a-9580-30d5a718d925";
        _cubeCobraClientMock.Setup(x => x.GetCardsInHtml(It.IsAny<string>()))
            .ReturnsAsync("<html><body>No cube data here</body></html>");

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        Assert.Null(result);
    }
}

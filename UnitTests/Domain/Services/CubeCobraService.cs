using Microsoft.Extensions.Logging;

using Moq;

using Domain.Clients;
using Domain.Models.DTO.Moxfield;
using Domain.Services;

namespace UnitTests.Domain.Services;

public class CubeCObraServiceTests
{
    private readonly CubeCobraService _service;

    public CubeCObraServiceTests()
    {
        _service = new CubeCobraService();
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
}

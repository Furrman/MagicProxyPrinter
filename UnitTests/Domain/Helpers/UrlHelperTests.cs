using Domain.Helpers;

namespace UnitTests.Domain.Helpers;

public class UrlHelperTests
{
    [Theory]
    [InlineData("https://api.scryfall.com/cards/d3a5e1f9-4c3e-4f9f-9e5e-5b3a2c1d0f8a")]
    [InlineData("https://api.scryfall.com/cards/d3a5e1f9-4c3e-4f9f-9e5e-5b3a2c1d0f8a/")]
    public void GetGuidFromLastPartOfUrl_ValidGuidSegment_ReturnsGuid(string url)
    {
        var expected = Guid.Parse("d3a5e1f9-4c3e-4f9f-9e5e-5b3a2c1d0f8a");

        var result = UrlHelper.GetGuidFromLastPartOfUrl(url);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetGuidFromLastPartOfUrl_NonGuidSegment_ReturnsNull()
    {
        var result = UrlHelper.GetGuidFromLastPartOfUrl("https://api.scryfall.com/cards/not-a-guid");

        Assert.Null(result);
    }
}

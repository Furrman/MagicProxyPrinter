using FluentAssertions;
using Moq;

using Domain.Factories;
using Domain.Services;

namespace UnitTests.Domain.Factories;

public class ServiceFactoryTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly IServiceFactory _serviceFactory;
    
    public ServiceFactoryTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceFactory = new ServiceFactory(
            _serviceProviderMock.Object
        );
    }

    [Theory]
    [InlineData("archidekt.com/decks/123456/test")]
    [InlineData("www.archidekt.com/decks/123456/test")]
    [InlineData("https://archidekt.com/decks/123456/test")]
    [InlineData("https://www.archidekt.com/decks/123456/test")]
    public void GetDeckRetriever_WithArchidektUrl_ReturnsArchidektServiceObject(string deckUrl)
    {
        // Arrange
        _serviceProviderMock.Setup(x => x.GetService(typeof(IArchidektService)))
            .Returns(new Mock<IArchidektService>().Object);

        // Act
        var deckRetriever = _serviceFactory.GetDeckBuildService(deckUrl);

        // Assert
        _serviceProviderMock.Verify(x => x.GetService(typeof(IArchidektService)), Times.Once);
        deckRetriever.Should().NotBeNull();
    }

    [Theory]
    [InlineData("edhrec.com/deckpreview/7VNuM_Ce5b3JbQrhfTsObA")]
    [InlineData("www.edhrec.com/deckpreview/7VNuM_Ce5b3JbQrhfTsObA")]
    [InlineData("https://edhrec.com/deckpreview/7VNuM_Ce5b3JbQrhfTsObA")]
    [InlineData("https://www.edhrec.com/deckpreview/7VNuM_Ce5b3JbQrhfTsObA")]
    public void GetDeckRetriever_WithEdhrecUrl_ReturnsEdhrecServiceObject(string deckUrl)
    {
        // Arrange
        _serviceProviderMock.Setup(x => x.GetService(typeof(IEdhrecService)))
            .Returns(new Mock<IEdhrecService>().Object);
        
        // Act
        var deckRetriever = _serviceFactory.GetDeckBuildService(deckUrl);

        // Assert
        _serviceProviderMock.Verify(x => x.GetService(typeof(IEdhrecService)), Times.Once);
        deckRetriever.Should().NotBeNull();
    }

    [Theory]
    [InlineData("moxfield.com/decks/123456/test")]
    [InlineData("www.moxfield.com/decks/123456/test")]
    [InlineData("https://moxfield.com/decks/123456/test")]
    [InlineData("https://www.moxfield.com/decks/123456/test")]
    public void GetDeckRetriever_WithMoxfieldUrl_ReturnsMoxfieldServiceObject(string deckUrl)
    {
        // Arrange
        _serviceProviderMock.Setup(x => x.GetService(typeof(IMoxfieldService)))
            .Returns(new Mock<IMoxfieldService>().Object);

        // Act
        var deckRetriever = _serviceFactory.GetDeckBuildService(deckUrl);

        // Assert
        _serviceProviderMock.Verify(x => x.GetService(typeof(IMoxfieldService)), Times.Once);
    }

    [Fact]
    public void GetDeckRetriever_WithNotMatchingUrl_ReturnsNull()
    {
        // Arrange
        string deckUrl = "";

        // Act
        var deckRetriever = _serviceFactory.GetDeckBuildService(deckUrl);

        // Assert
        deckRetriever.Should().BeNull();
    }
}
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

    [Fact]
    public void GetDeckRetriever_WithArchidektUrl_ReturnsArchidektServiceObject()
    {
        // Arrange
        string deckUrl = "https://archidekt.com/decks/123456/test";
        _serviceProviderMock.Setup(x => x.GetService(typeof(IArchidektService)))
            .Returns(new Mock<IArchidektService>().Object);

        // Act
        var deckRetriever = _serviceFactory.GetDeckBuildService(deckUrl);

        // Assert
        _serviceProviderMock.Verify(x => x.GetService(typeof(IArchidektService)), Times.Once);
        deckRetriever.Should().NotBeNull();
    }

    [Fact]
    public void GetDeckRetriever_WithEdhrecUrl_ReturnsEdhrecServiceObject()
    {
        // Arrange
        string deckUrl = "https://edhrec.com/deckpreview/7VNuM_Ce5b3JbQrhfTsObA";
        _serviceProviderMock.Setup(x => x.GetService(typeof(IEdhrecService)))
            .Returns(new Mock<IEdhrecService>().Object);
        
        // Act
        var deckRetriever = _serviceFactory.GetDeckBuildService(deckUrl);

        // Assert
        _serviceProviderMock.Verify(x => x.GetService(typeof(IEdhrecService)), Times.Once);
        deckRetriever.Should().NotBeNull();
    }

    [Fact]
    public void GetDeckRetriever_WithMoxfieldUrl_ReturnsMoxfieldServiceObject()
    {
        // Arrange
        string deckUrl = "https://moxfield.com/decks/123456/test";
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
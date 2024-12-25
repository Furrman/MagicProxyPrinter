using FluentAssertions;
using Moq;

using Domain.Factories;
using Domain.Services;

namespace UnitTests.Domain.Factories;

public class DeckRetrieverFactoryTests
{
    private readonly Mock<IArchidektService> _archidektServiceMock;
    private readonly Mock<IEdhrecService> _edhrecServiceMock;
    private readonly Mock<IMoxfieldService> _moxfieldServiceMock;
    private readonly IDeckRetrieverFactory _deckRetrieverFactory;
    
    public DeckRetrieverFactoryTests()
    {
        _archidektServiceMock = new Mock<IArchidektService>();
        _edhrecServiceMock = new Mock<IEdhrecService>();
        _moxfieldServiceMock = new Mock<IMoxfieldService>();

        _deckRetrieverFactory = new DeckRetrieverFactory(
            _archidektServiceMock.Object,
            _edhrecServiceMock.Object,
            _moxfieldServiceMock.Object
        );
    }

    [Fact]
    public void GetDeckRetriever_WithArchidektUrl_ReturnsArchidektServiceObject()
    {
        // Arrange
        string deckUrl = "https://archidekt.com/decks/123456/test";

        _archidektServiceMock.Setup(x => x.TryExtractDeckIdFromUrl(deckUrl, out It.Ref<int>.IsAny))
            .Returns(true);

        // Act
        var deckRetriever = _deckRetrieverFactory.GetDeckRetriever(deckUrl);

        // Assert
        _archidektServiceMock.Verify(x => x.TryExtractDeckIdFromUrl(It.IsAny<string>(), out It.Ref<int>.IsAny), Times.Once);
        deckRetriever.Should().NotBeNull();
        deckRetriever.Should().Be(_archidektServiceMock.Object);
    }

    [Fact]
    public void GetDeckRetriever_WithEdhrecUrl_ReturnsEdhrecServiceObject()
    {
        // Arrange
        string deckUrl = "https://edhrec.com/deckpreview/7VNuM_Ce5b3JbQrhfTsObA";

        _archidektServiceMock.Setup(x => x.TryExtractDeckIdFromUrl(deckUrl, out It.Ref<int>.IsAny))
            .Returns(false);
        _edhrecServiceMock.Setup(x => x.TryExtractDeckIdFromUrl(deckUrl, out It.Ref<string>.IsAny))
            .Returns(true);
        _moxfieldServiceMock.Setup(x => x.TryExtractDeckIdFromUrl(deckUrl, out It.Ref<string>.IsAny))
            .Returns(false);

        // Act
        var deckRetriever = _deckRetrieverFactory.GetDeckRetriever(deckUrl);

        // Assert
        _edhrecServiceMock.Verify(x => x.TryExtractDeckIdFromUrl(It.IsAny<string>(), out It.Ref<string>.IsAny), Times.Once);
        deckRetriever.Should().NotBeNull();
        deckRetriever.Should().Be(_edhrecServiceMock.Object);
    }

    [Fact]
    public void GetDeckRetriever_WithMoxfieldUrl_ReturnsMoxfieldServiceObject()
    {
        // Arrange
        string deckUrl = "https://moxfield.com/decks/123456/test";

        _archidektServiceMock.Setup(x => x.TryExtractDeckIdFromUrl(deckUrl, out It.Ref<int>.IsAny))
            .Returns(false);
        _edhrecServiceMock.Setup(x => x.TryExtractDeckIdFromUrl(deckUrl, out It.Ref<string>.IsAny))
            .Returns(false);
        _moxfieldServiceMock.Setup(x => x.TryExtractDeckIdFromUrl(deckUrl, out It.Ref<string>.IsAny))
            .Returns(true);

        // Act
        var deckRetriever = _deckRetrieverFactory.GetDeckRetriever(deckUrl);

        // Assert
        _moxfieldServiceMock.Verify(x => x.TryExtractDeckIdFromUrl(It.IsAny<string>(), out It.Ref<string>.IsAny), Times.Once);
        deckRetriever.Should().NotBeNull();
        deckRetriever.Should().Be(_moxfieldServiceMock.Object);
    }

    [Fact]
    public void GetDeckRetriever_WithNotMatchingUrl_ReturnsNull()
    {
        // Arrange
        string deckUrl = "";

        _archidektServiceMock.Setup(x => x.TryExtractDeckIdFromUrl(deckUrl, out It.Ref<int>.IsAny))
            .Returns(false);

        // Act
        var deckRetriever = _deckRetrieverFactory.GetDeckRetriever(deckUrl);

        // Assert
        _archidektServiceMock.Verify(x => x.TryExtractDeckIdFromUrl(It.IsAny<string>(), out It.Ref<int>.IsAny), Times.Once);
        deckRetriever.Should().BeNull();
    }
}
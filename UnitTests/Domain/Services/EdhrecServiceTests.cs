using Microsoft.Extensions.Logging;

using FluentAssertions;
using Moq;

using Domain.Clients;
using Domain.Models.DTO;
using Domain.Services;

namespace UnitTests.Domain.Services;

public class EdhrecServiceTests
{
    private readonly Mock<IEdhrecClient> _edhrecClientMock;
    private readonly Mock<IArchidektService> _archidektServiceMock;
    private readonly Mock<ILogger<EdhrecService>> _loggerMock;
    private readonly EdhrecService _service;

    public EdhrecServiceTests()
    {
        _edhrecClientMock = new Mock<IEdhrecClient>();
        _archidektServiceMock = new Mock<IArchidektService>();
        _loggerMock = new Mock<ILogger<EdhrecService>>();
        _service = new EdhrecService(_edhrecClientMock.Object, 
            _archidektServiceMock.Object,
            _loggerMock.Object);
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
    
    [Fact]
    public async Task RetrieveDeckFromWeb_WithValidDeckId_SetDeckName()
    {
        // Arrange
        string deckUrl = "https://edhrec.com/deckpreview/7VNuM_Ce5b3JbQrhfTsObA";
        _edhrecClientMock.Setup(x => x.GetDeck(It.IsAny<string>())).ReturnsAsync("""
            <html lang="en">
            	<body>
            		<div id="__next">
            			<main class="Main_main__Kkd1U">
            				<div class="Leaderboard_container__2JclS">
            					<div class="Leaderboard_leaderboard__5X5XE">
            						<div class="lazyload-wrapper h-100">
            							<div class="lazyload-placeholder"/>
            						</div>
            					</div>
            					<div class="mvLeaderboard"/>
            				</div>
            				<div class="d-flex flex-grow-1 p-3 pe-lg-0">
            					<div class="d-flex w-100">
            						<div class="Main_left__B9nka">
            							<div class="Container_container__A7FAx">
            								<div class="CoolHeader_container__MASgl card shadow-sm">
            									<h3 class="m-2">Deck with Commodore Guff</h3>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </main>
                    </div>
                </body> 
            </html>
            """);

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Deck with Commodore Guff");
    }

    [Fact]
    public async Task RetrieveDeckFromWeb_WithValidDeckId_SetCardDetails()
    {
        // Arrange
        string deckUrl = "https://edhrec.com/deckpreview/7VNuM_Ce5b3JbQrhfTsObA";
        _edhrecClientMock.Setup(x => x.GetDeck(It.IsAny<string>())).ReturnsAsync("""
            <html lang="en">
                <body>
                    <div id="__next">
                        <main class="Main_main__Kkd1U">
                            <div class="d-flex flex-grow-1 p-3 pe-lg-0">
                                <div class="d-flex w-100">
                                    <div class="Main_left__B9nka">
                                        <div class="mvCardList shadow-sm CardLists_mvCardList__chTPm card">
                                            <a href="https://www.cardkingdom.com/builder?c=2+Arena+Rector%0D%0A2+Arcane+Signet%0D%0A10+Llanowar+Elves%0D%0A" 
                                               class="CardLists_top__10jEa" id="cardlists"></a>
                                            <div class="d-flex">
                                                <div class="position-relative w-100">
                                                    <div class="m-2">
                                                        <div class="flex-grow-1">
                                                            <div class="Grid_grid__EAPIs">
                                                                <div class="Grid_cardlist__AXXsz" id="creature" style="grid-column:span 0">
                                                                    <div>
                                                                        <div class="Grid_grid__EAPIs">
                                                                            <div class="d-flex justify-content-center mb-2">
                                                                                <div class="Card_container__Ng56K">
                                                                                    <div class="Card_nameWrapper__oeNTe">
                                                                                        <span class="Card_name__Mpa7S">Arena Rector</span>
                                                                                    </div>
                                                                                    <div class="lazyload-wrapper">
                                                                                        <div style="height:359.19px" class="lazyload-placeholder"></div>
                                                                                    </div>
                                                                                </div>
                                                                            </div>
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </main>
                    </div>
                </body> 
            </html>
            """);

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        result.Should().NotBeNull();
        result!.Cards[0].Should().NotBeNull();
        result.Cards[0].Name.Should().Be("Arena Rector");
        // Quantity in EDHRec should be 1 by default
        result.Cards[0].Quantity.Should().Be(1);
    }
    
    [Fact]
    public async Task RetrieveDeckFromWeb_WithArchidektLink_GetDeckFromArchidektService()
    {
        // Arrange
        string deckUrl = "https://edhrec.com/deckpreview/7VNuM_Ce5b3JbQrhfTsObA";
        _archidektServiceMock.Setup(x => x.RetrieveDeckFromWeb(It.IsAny<string>()))
            .ReturnsAsync(new DeckDetailsDTO());
        _edhrecClientMock.Setup(x => x.GetDeck(It.IsAny<string>())).ReturnsAsync("""
            <html lang="en">
                <body>
                    <div id="__next">
                        <main class="Main_main__Kkd1U">
                            <div class="d-flex flex-grow-1 p-3 pe-lg-0">
                                <div class="d-flex w-100">
                                    <div class="Main_left__B9nka">
                                        <div class="Panels_container__jvZjo">
                                            <div class="Panels_panels__t_RbF">
                                                <div class="Panels_row__GFZm_">
                                                    <div class="Panels_rowGroup__0xwUE Panels_right__stwAo">
                                                        <div class="flex-grow-1 shadow-sm w-100 card">
                                                            <div class="d-flex flex-column h-100 justify-content-between m-3">
                                                                <div>Source: <a href="https://archidekt.com/decks/9146588?utm_source=edhrec&amp;utm_medium=deck_summary" rel="noopener noreferrer" target="_blank" title="Deck Source">archidekt.com</a>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>                 
                                    </div>
                                </div>
                            </div>
                        </main>
                    </div>
                </body> 
            </html>
            """);

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        result.Should().NotBeNull();
    }
}
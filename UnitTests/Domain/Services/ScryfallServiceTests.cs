using Microsoft.Extensions.Logging;

using Moq;

using Domain.Clients;
using Domain.Models.DTO;
using Domain.Models.DTO.Scryfall;
using Domain.Services;
using Domain.IO;
using Domain.Constants;

namespace UnitTests.Domain.Services;

public class ScryfallServiceTests
{
    private readonly Mock<IScryfallClient> _scryfallClientMock;
    private readonly Mock<IFileManager> _fileManagerMock;
    private readonly Mock<ILogger<ScryfallService>> _loggerMock;
    private readonly ScryfallService _service;

    public ScryfallServiceTests()
    {
        _scryfallClientMock = new Mock<IScryfallClient>();
        _fileManagerMock = new Mock<IFileManager>();
        _loggerMock = new Mock<ILogger<ScryfallService>>();
        _service = new ScryfallService(_scryfallClientMock.Object, 
            _fileManagerMock.Object,
            _loggerMock.Object);
    }


    [Fact]
    public async Task DownloadCardSideImage_WhenImageUrlProvided_ShouldTryToDownloadImage()
    {
        // Arrange
        var imageUrl = "https://example.com/image.jpg";
        var folderPath = "/path/to/folder";
        var filename = "card";
        var quantity = 1;
        var imageBytes = new byte[] { 0x12, 0x34, 0x56, 0x78 };

        _scryfallClientMock.Setup(api => api.DownloadImage(imageUrl)).ReturnsAsync(imageBytes);

        // Act
        await _service.DownloadCardSideImage(imageUrl, folderPath, filename, quantity);

        // Assert
        _scryfallClientMock.Verify(api => api.DownloadImage(imageUrl), Times.Once);
    }
    
    [Fact]
    public async Task DownloadCardSideImage_WhenImageBytesAreNotNull_ShouldDownloadImage()
    {
        // Arrange
        var imageUrl = "https://example.com/image.jpg";
        var folderPath = "/path/to/folder";
        var filename = "card";
        var quantity = 1;
        var imageBytes = new byte[] { 0x12, 0x34, 0x56, 0x78 };

        _scryfallClientMock.Setup(api => api.DownloadImage(imageUrl)).ReturnsAsync(imageBytes);

        // Act
        await _service.DownloadCardSideImage(imageUrl, folderPath, filename, quantity);

        // Assert
        _fileManagerMock.Verify(fm => fm.CreateImageFile(imageBytes, folderPath, It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public async Task DownloadCardSideImage_WhenImageBytesAreNull_ShouldNotDownloadImage()
    {
        // Arrange
        var imageUrl = "https://example.com/image.jpg";
        var folderPath = "/path/to/folder";
        var filename = "card";
        var quantity = 1;
        var imageBytes = null as byte[];

        _scryfallClientMock.Setup(api => api.DownloadImage(imageUrl)).ReturnsAsync(imageBytes);

        // Act
        await _service.DownloadCardSideImage(imageUrl, folderPath, filename, quantity);

        // Assert
        _fileManagerMock.Verify(fm => fm.CreateImageFile(imageBytes!, folderPath, It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCardImageLinks_WhenCardsAreProvided_ShouldSetCardImageLinks()
    {
        // Arrange
        List<CardEntryDTO> cards = new List<CardEntryDTO>
        {
            new() { Name = "Card 1" }
        };

        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO()
                {
                    Data = [new CardDataDTO { Name = "Card 1", ImageUriData = new CardImageUriDTO() { Large = "https://example.com/card1.jpg" }}]
                });

        // Act
        await _service.UpdateCardImageLinks(cards);

        // Assert
        _scryfallClientMock.Verify(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        Assert.Equal("https://example.com/card1.jpg", cards[0].CardSides.First().ImageUrl);
    }

    [Fact]
    public async Task UpdateCardImageLinks_WhenCardsIdsAreProvided_ShouldSetCardImageLinks()
    {
        // Arrange
        List<CardEntryDTO> cards =
        [
            new() { Id = Guid.NewGuid() }
        ];

        _scryfallClientMock.Setup(api => api.GetCard(It.IsAny<Guid>()))
            .ReturnsAsync(new CardDataDTO { Name = "Card 1", ImageUriData = new CardImageUriDTO() { Large = "https://example.com/card1.jpg" } });

        // Act
        await _service.UpdateCardImageLinks(cards);

        // Assert
        _scryfallClientMock.Verify(api => api.GetCard(It.IsAny<Guid>()), Times.Once);
        Assert.Equal("https://example.com/card1.jpg", cards[0].CardSides.First().ImageUrl);
    }

    [Fact]
    public async Task UpdateCardImageLinks_WhenCardNotFound_ShouldNotUpdateCardImageLinks()
    {
        // Arrange
        List<CardEntryDTO> cards =
        [
            new() { Name = "Card 1" }
        ];

        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync((CardSearchDTO?)null);

        // Act
        await _service.UpdateCardImageLinks(cards);

        // Assert
        _scryfallClientMock.Verify(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        Assert.Empty(cards[0].CardSides);
    }

    [Fact]
    public async Task UpdateCardImageLinks_WhenCardIsDualSide_ShouldUpdateBothPagesCardImageLinks()
    {
        // Arrange
        List<CardEntryDTO> cards = new List<CardEntryDTO>
        {
            new() { Name = "Card 1" }
        };

        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO()
            {
            Data =
            [
                new CardDataDTO 
                { 
                    Name = "Card 1", 
                    CardFaces =
                    [
                        new CardFaceDTO () { Name = "Front", ImageUriData = new CardImageUriDTO() { Large = "https://example.com/card1.jpg" } },
                        new CardFaceDTO () { Name = "Back", ImageUriData = new CardImageUriDTO() { Large = "https://example.com/card1-back.jpg" } }
                    ]
                }
            ]
            });

        // Act
        await _service.UpdateCardImageLinks(cards);

        // Assert
        _scryfallClientMock.Verify(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        Assert.NotEmpty(cards[0].CardSides);
        Assert.Equal(2, cards[0].CardSides.Count);
    }

    [Fact]
    public async Task UpdateCardImageLinks_WhenCardIsArt_ShouldUpdateFirstPageCardImageLink()
    {
        // Arrange
        List<CardEntryDTO> cards = new List<CardEntryDTO>
        {
            new() { Name = "Card 1", Art = true }
        };

        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO()
            {
                Data =
                [
                    new CardDataDTO 
                    { 
                        Name = "Card 1", 
                        CardFaces =
                        [
                            new CardFaceDTO () { Name = "Front", ImageUriData = new CardImageUriDTO() { Large = "https://example.com/card1.jpg" } },
                            new CardFaceDTO () { Name = "Back", ImageUriData = new CardImageUriDTO() { Large = "https://example.com/card1-back.jpg" } }
                        ]}
                ]
            });

        // Act
        await _service.UpdateCardImageLinks(cards);

        // Assert
        _scryfallClientMock.Verify(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        Assert.NotEmpty(cards[0].CardSides);
        Assert.Single(cards[0].CardSides);
        Assert.Equal("https://example.com/card1.jpg", cards[0].CardSides.First().ImageUrl);
    }

    [Fact]
    public async Task UpdateCardImageLinks_WhenTokenCopyNumberSpecified_ShouldAddTokens()
    {
        // Arrange
        List<CardEntryDTO> cards = new List<CardEntryDTO>
        {
            new() { Name = "Card 1" }
        };

        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO()
            {
                Data =
                [
                    new CardDataDTO
                    {
                        Name = "Card 1", 
                        AllParts = [new CardPartDTO() { Name = "Card 1 Token", Component = ScryfallParts.COMPONENT_TOKEN, Uri = "https://example.com/card1-token.jpg" }]
                    }
                ]
            });

        // Act
        await _service.UpdateCardImageLinks(cards, tokenCopies: 1);

        // Assert
        Assert.Single(cards[0].Tokens);
        Assert.Equal("https://example.com/card1-token.jpg", cards[0].Tokens.First().Uri);
        Assert.Equal("Card 1 Token", cards[0].Tokens.First().Name);
    }

    [Fact]
    public async Task UpdateCardImageLinks_WhenTokenCopyNumberNotSet_ShouldNotAddTokens()
    {
        // Arrange
        List<CardEntryDTO> cards =
        [
            new() { Name = "Card 1" }
        ];

        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO()
            {
                Data =
                [
                    new CardDataDTO
                    {
                        Name = "Card 1", 
                        AllParts = [new CardPartDTO() { Name = "Card 1 Token", Component = ScryfallParts.COMPONENT_TOKEN, Uri = "https://example.com/card1-token.jpg" }]
                    }
                ]
            });

        // Act
        await _service.UpdateCardImageLinks(cards);

        // Assert
        Assert.Empty(cards[0].Tokens);
    }

    [Fact]
    public async Task UpdateCardImageLinks_WhenTokenCopyNumberSpecified_ShouldAddTokensSpecifiedNumberOfTimesToOtherCardsForPrinting()
    {
        // Arrange
        List<CardEntryDTO> cards = new List<CardEntryDTO>
        {
            new() { Name = "Card 1" }
        };
        var tokenId = Guid.NewGuid();

        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO()
            {
                Data =
                [
                    new CardDataDTO
                    {
                        Name = "Card 1", 
                        AllParts = [new CardPartDTO() { Name = "Card 1 Token", Component = ScryfallParts.COMPONENT_TOKEN, Uri = $"https://example.com/{tokenId}" }]
                    }
                ]
            });
        _scryfallClientMock.Setup(api => api.GetCard(tokenId))
            .ReturnsAsync(new CardDataDTO { Name = "Card 1 Token", ImageUriData = new CardImageUriDTO() { Large = "https://example.com/card1-token.jpg" } });

        // Act
        await _service.UpdateCardImageLinks(cards, tokenCopies: 3);

        // Assert
        var tokenCard = cards.FirstOrDefault(c => c.Name == "Card 1 Token");
        Assert.NotNull(tokenCard);
        Assert.Equal(3, tokenCard.Quantity);
        Assert.Equal("https://example.com/card1-token.jpg", tokenCard.CardSides.First().ImageUrl);
    }

    [Fact]
    public async Task UpdateCardImageLinks_WhenTokenCopyNumberNotSpecified_ShouldNotAddAnyTokensToOtherCardsForPrinting()
    {
        // Arrange
        List<CardEntryDTO> cards = new List<CardEntryDTO>
        {
            new() { Name = "Card 1" }
        };
        var tokenId = Guid.NewGuid();

        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO()
            {
                Data =
                [
                    new CardDataDTO
                    {
                        Name = "Card 1", 
                        AllParts = [new CardPartDTO() { Name = "Card 1 Token", Component = ScryfallParts.COMPONENT_TOKEN, Uri = $"https://example.com/{tokenId}" }]
                    }
                ]
            });
        _scryfallClientMock.Setup(api => api.GetCard(tokenId))
            .ReturnsAsync(new CardDataDTO { Name = "Card 1 Token", ImageUriData = new CardImageUriDTO() { Large = "https://example.com/card1-token.jpg" } });

        // Act
        await _service.UpdateCardImageLinks(cards);

        // Assert
        Assert.DoesNotContain(cards, c => c.Name == "Card 1 Token" && c.Quantity == 3 && c.CardSides.First().ImageUrl == "https://example.com/card1-token.jpg");
    }
    
    [Fact]
    public async Task UpdateCardImageLinks_WhenTokenCopyNumberSpecifiedAndGroupingTokensSet_ShouldNotAddTheSameTokenMoreTimesThanSpecified()
    {
        // Arrange
        List<CardEntryDTO> cards = new List<CardEntryDTO>
        {
            new() { Name = "Card 1" },
            new() { Name = "Card 2" }
        };
        var tokenId = Guid.NewGuid();
        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO()
            {
                Data =
                [
                    new CardDataDTO
                    {
                        Name = "Card 1", 
                        AllParts = [new CardPartDTO() { Name = "Token", Component = ScryfallParts.COMPONENT_TOKEN, Uri = $"https://example.com/{tokenId}" }]
                    },
                    new CardDataDTO
                    {
                        Name = "Card 2", 
                        AllParts = [new CardPartDTO() { Name = "Token", Component = ScryfallParts.COMPONENT_TOKEN, Uri = $"https://example.com/{tokenId}" }]
                    }
                ]
            });
        _scryfallClientMock.Setup(api => api.GetCard(tokenId))
            .ReturnsAsync(new CardDataDTO { Name = "Token", ImageUriData = new CardImageUriDTO() { Large = "https://example.com/token.jpg" } });
        
        // Act
        await _service.UpdateCardImageLinks(cards, tokenCopies: 1, groupTokens: true);

        // Assert
        var tokenCard = cards.FirstOrDefault(card => card.Name == "Token");
        Assert.NotNull(tokenCard);
        Assert.Equal(1, tokenCard.Quantity);
    }
    
    [Fact]
    public async Task UpdateCardImageLinks_WhenTokenCopyNumberSpecifiedAndGroupingTokensUnset_ShouldAddTheSameTokenMoreTimesThanSpecified()
    {
        // Arrange
        List<CardEntryDTO> cards = new List<CardEntryDTO>
        {
            new() { Name = "Card 1" },
            new() { Name = "Card 2" }
        };
        var tokenId = Guid.NewGuid();
        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO()
            {
                Data =
                [
                    new CardDataDTO
                    {
                        Name = "Card 1", 
                        AllParts = [new CardPartDTO() { Name = "Token", Component = ScryfallParts.COMPONENT_TOKEN, Uri = $"https://example.com/{tokenId}" }]
                    },
                    new CardDataDTO
                    {
                        Name = "Card 2", 
                        AllParts = [new CardPartDTO() { Name = "Token", Component = ScryfallParts.COMPONENT_TOKEN, Uri = $"https://example.com/{tokenId}" }]
                    }
                ]
            });
        _scryfallClientMock.Setup(api => api.GetCard(tokenId))
            .ReturnsAsync(new CardDataDTO { Name = "Token", ImageUriData = new CardImageUriDTO() { Large = "https://example.com/token.jpg" } });
        
        // Act
        await _service.UpdateCardImageLinks(cards, tokenCopies: 1, groupTokens: false);

        // Assert
        Assert.Equal(2, cards.Count(card => card.Name == "Token"));
    }
}
using Microsoft.Extensions.Logging;

using Domain.Clients;
using Domain.Models.DTO;
using Domain.Models.Events;
using Domain.Models.DTO.Scryfall;
using Domain.Constants;
using Domain.Helpers;
using Domain.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.Services;

/// <summary>
/// Represents a service for interacting with Magic cards via Scryfall.
/// </summary>
public interface IScryfallService
{
    /// <summary>
    /// Event that is raised to report the progress of getting deck details.
    /// </summary>
    event EventHandler<GetDeckDetailsProgressEventArgs>? GetDeckDetailsProgress;

    /// <summary>
    /// Downloads the image of a card side.
    /// </summary>
    /// <param name="imageUrl">The URL of the image to download.</param>
    /// <param name="folderPath">The path to the folder where the image will be saved.</param>
    /// <param name="filename">The name of the image file.</param>
    /// <param name="quantity">The quantity of the card side.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DownloadCardSideImage(string imageUrl, string folderPath, string filename, int quantity);

    /// <summary>
    /// Updates the card image links.
    /// </summary>
    /// <param name="cards">The list of card entries to update.</param>
    /// <param name="languageCode">The language code for localization (optional).</param>
    /// <param name="tokenCopies">Quantity of each token (optional).</param>
    /// <param name="groupTokens">Group tokens based on name (optional).</param>
    /// <param name="includeEmblems">Include emblems to documents (optional).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateCardImageLinks(List<CardEntryDTO> cards, 
        string? languageCode = null, int tokenCopies = 0, bool groupTokens = false, bool includeEmblems = false);
}

public class ScryfallService(IScryfallClient scryfallApiClient, 
    IFileManager fileManager,
    ILogger<ScryfallService> logger) 
    : IScryfallService
{
    public event EventHandler<GetDeckDetailsProgressEventArgs>? GetDeckDetailsProgress;

    private readonly IScryfallClient _scryfallApiClient = scryfallApiClient;
    private readonly IFileManager _fileManager = fileManager;
    private readonly ILogger<ScryfallService> _logger = logger;


    public async Task DownloadCardSideImage(string imageUrl, string folderPath, string filename, int quantity)
    {
        try
        {
            var imageBytes = await _scryfallApiClient.DownloadImage(imageUrl);
            if (imageBytes is null)
            {
                _logger.LogWarning("Image not received from internet");
                return;
            }

            await _fileManager.CreateImageFile(imageBytes, folderPath, $"{quantity}_{filename.Replace(" // ", "-")}.jpg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in downloading image from the Scryfall");
        }
    }

    public async Task UpdateCardImageLinks(List<CardEntryDTO> cards, 
        string? languageCode = null, int tokenCopies = 0, bool groupTokens = false, bool includeEmblems = false)
    {
        try
        {
            int count = cards.Count;
            int step = UpdateStep(0, count);
            foreach (var card in cards)
            {
                var images = await GetCardImageUrls(card, languageCode: languageCode, 
                    getTokens: tokenCopies > 0, includeEmblems: includeEmblems);
                if (images != null)
                {
                    card.CardSides = images;
                }
                step = UpdateStep(step, count);
            }

            await UpdateTokens(cards, tokenCopies, groupTokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in downloading cards from the Scryfall");
        }
    }


    private async Task<HashSet<CardSideDTO>?> GetCardImageUrls(
        CardEntryDTO card, 
        string? languageCode = null, 
        bool getTokens = false,
        bool includeEmblems = false)
    {
        var scryfallCardData = await SearchCard(card, languageCode);
        // If card was not found, try to search it without language code
        if (scryfallCardData is null && languageCode is not null)
        {
            _logger.LogWarning("Card {Name} in [{languageCode}] was not found in the Scryfall database", card.Name, languageCode);
            scryfallCardData = await SearchCard(card);
        }
        if (scryfallCardData is null)
        {
            _logger.LogError("Card {Name} was not found in the Scryfall database and will be ignored", card.Name);
            return null;
        }

        HashSet<CardSideDTO>? cardSides = IsDualSideCard(scryfallCardData) 
            ? GetDualSideCardLinks(card, scryfallCardData) 
            : GetSingleSideCardLink(scryfallCardData);

        if (getTokens)
        {
            AddRelatedTokensToCardImages(card, scryfallCardData);
        }
        if (includeEmblems)
        {
            AddRelatedEmblemsToCardImages(card, scryfallCardData);
        }

        return cardSides;
    }

    private async Task<CardDataDTO?> SearchCard(CardEntryDTO card, string? languageCode = null)
    {
        if (card.Id != null)
        {
            return await _scryfallApiClient.GetCard(card.Id.Value);
        }

        // Check if we have enough details about specific card for a Find instead of Search
        var cardSearch = card.ExpansionCode != null && card.CollectorNumber != null
                ? new() { Data = [await _scryfallApiClient.FindCard(card.Name, card.ExpansionCode, card.CollectorNumber, languageCode)] }
                : await _scryfallApiClient.SearchCard(card.Name, card.ExpansionCode is not null || card.Etched || card.Art, languageCode != null);

        // Look for searched card in the search result
        var searchedCard = cardSearch?.Data?.FirstOrDefault(c =>
            c != null && c.Name != null && (c.Name.Equals(card.Name, StringComparison.OrdinalIgnoreCase) // Find by name
                || c.Name.Split(" // ").Any(n => n.Equals(card.Name, StringComparison.OrdinalIgnoreCase))) // Find by name in dual side card
            && ((!card.Etched) || (card.Etched && c.TcgPlayerEtchedId is not null)) // Find etched frame if required
            && ((card.ExpansionCode is null) || string.Equals(card.ExpansionCode, c.Set)) // Find by expansion if required
            && ((languageCode is null) || c.Lang!.Equals(languageCode, StringComparison.OrdinalIgnoreCase)) // Find by expansion if required
            );
        
        return searchedCard;
    }
    

    private bool IsArtCard(CardEntryDTO deckCard) => deckCard.Art;

    private bool IsDualSideCard(CardDataDTO scryfallCardData) => scryfallCardData.CardFaces is not null
        && scryfallCardData.ImageUriData is null; // If card has image and has card faces, it is Adventure card with one side

    private HashSet<CardSideDTO> GetDualSideCardLinks(CardEntryDTO deckCard, CardDataDTO scryfallCardData)
    {
        if (IsArtCard(deckCard))
        {
            return GetArtSideOnlyCardLink(scryfallCardData);
        }

        var cardSides = new HashSet<CardSideDTO>();
        foreach (var cardFace in scryfallCardData.CardFaces!)
        {
            if (cardFace.ImageUriData is null)
            {
                continue;
            }

            cardSides.Add(new CardSideDTO { Name = cardFace.Name ?? string.Empty, ImageUrl = cardFace.ImageUriData?.Large ?? string.Empty });
        }

        return cardSides;
    }
    
    private HashSet<CardSideDTO> GetArtSideOnlyCardLink(CardDataDTO scryfallCardData)
    {
        var cardSides = new HashSet<CardSideDTO>
        {
            new() { Name = scryfallCardData.Name ?? string.Empty, ImageUrl = scryfallCardData.CardFaces?.FirstOrDefault()?.ImageUriData?.Large ?? string.Empty }
        };
        return cardSides;
    }

    private HashSet<CardSideDTO>? GetSingleSideCardLink(CardDataDTO scryfallCardData)
    {
        if (scryfallCardData.ImageUriData?.Large is null)
        {
            _logger.LogError("Card {Name} does not have any url to its picture", scryfallCardData.Name);
            return null;
        }

        HashSet<CardSideDTO> cardSides =
        [
            new CardSideDTO
            {
                Name = scryfallCardData.Name ?? string.Empty, ImageUrl = scryfallCardData.ImageUriData.Large
            }
        ];
        return cardSides;
    }

    private void AddRelatedTokensToCardImages(CardEntryDTO card, CardDataDTO searchedCard)
    {
        var allParts = searchedCard!.AllParts?.Where(p => p.Component == ScryfallParts.COMPONENT_TOKEN);
        if (allParts is not null)
        {
            foreach (var part in allParts)
            {
                card.Tokens.Add(new CardTokenDTO
                {
                    Id = part.Id,
                    Name = part.Name ?? string.Empty,
                    Uri = part.Uri ?? string.Empty
                });
            }
        }
    }

    private void AddRelatedEmblemsToCardImages(CardEntryDTO card, CardDataDTO searchedCard)
    {
        var allParts = searchedCard!.AllParts?.Where(p => 
            p.TypeLine?.Contains(ScryfallParts.TYPE_LINE_EMBLEM) ?? false);
        if (allParts is not null)
        {
            foreach (var part in allParts)
            {
                card.Tokens.Add(new CardTokenDTO
                {
                    Id = part.Id,
                    Name = part.Name ?? string.Empty,
                    Uri = part.Uri ?? string.Empty,
                    IsEmblem = true
                });
            }
        }
    }
    

    private async Task UpdateTokens(List<CardEntryDTO> cards, int tokenCopies, bool groupTokens)
    {
        // Casting to list, because it will be modified
        var tokens = cards.SelectMany(c => c.Tokens)
            .ToList();
        if (groupTokens)
        {
            tokens = tokens.GroupBy(t => t.Name)
                .Select(g => g.First())
                .ToList();
        }

        foreach (var token in tokens)
        {
            var tokenId = token.Id ?? UrlHelper.GetGuidFromLastPartOfUrl(token.Uri!);
            if (tokenId is null)
            {
                _logger.LogError("Token {Name} does not have a valid Scryfall reference", token.Name);
                continue;
            }
            var cardToken = await _scryfallApiClient.GetCard(tokenId.Value);
            if (cardToken is not null)
            {
                cards.Add(new CardEntryDTO
                {
                    Name = token.Name ?? string.Empty,
                    Quantity = token.IsEmblem ? 1 : tokenCopies,
                    ExpansionCode = cardToken.Set,
                    CardSides = [new CardSideDTO { Name = cardToken.Name!, ImageUrl = cardToken.ImageUriData?.Large ?? string.Empty }]
                });
            }
        }
    }

    
    private int UpdateStep(int step, int count)
    {
        var percent = (double)step / count * 100;
        GetDeckDetailsProgress?.Invoke(this, new GetDeckDetailsProgressEventArgs
        {
            Percent = percent
        });
        return ++step;
    }
}
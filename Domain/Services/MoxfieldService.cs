using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

using Domain.Clients;
using Domain.Factories;
using Domain.Models.DTO;

namespace Domain.Services;

public interface IMoxfieldService : IDeckRetriever
{
    /// <summary>
    /// Tries to extract the deck ID from the given URL.
    /// </summary>
    /// <param name="url">The URL to extract the deck ID from.</param>
    /// <param name="deckId">The extracted deck ID, if successful.</param>
    /// <returns><c>true</c> if the deck ID was successfully extracted; otherwise, <c>false</c>.</returns>
    bool TryExtractDeckIdFromUrl(string url, out string deckId);
}

public class MoxfieldService(IMoxfieldClient moxfieldClient, ILogger<MoxfieldService> logger) : IMoxfieldService
{
    private readonly IMoxfieldClient _moxfieldClient = moxfieldClient;
    private readonly ILogger<MoxfieldService> _logger = logger;

    public async Task<DeckDetailsDTO?> RetrieveDeckFromWeb(string deckUrl)
    {
        TryExtractDeckIdFromUrl(deckUrl, out string deckId);
        
        var deckDto = await _moxfieldClient.GetDeck(deckId);
        if (deckDto is null)
        {
            _logger.LogError("Deck not loaded from internet");
            return null;
        }

        var deck = new DeckDetailsDTO 
        {
            Name = deckDto!.Name ?? string.Empty,
            Cards = deckDto!.Boards?.SelectMany(x => x.Value.Cards?.Select(y => new CardEntryDTO
            {
                Id = Guid.TryParse(y.Value?.Card?.Scryfall_Id, out Guid guid) 
                    ? guid 
                    : null,
                Name = y.Value?.Card?.Name ?? string.Empty,
                Quantity = y.Value?.Quantity ?? 1
            }) ?? [])
            ?.ToList() ?? []
        };

        return deck;
    }
    
    public bool TryExtractDeckIdFromUrl(string url, out string deckId)
    {
        deckId = string.Empty;
        string pattern = @"^https:\/\/(www\.)?moxfield\.com\/decks\/([\w\-._~]+)\/?$";
        Regex regex = new(pattern);

        Match match = regex.Match(url);
        if (match.Success)
        {
            var index = match.Groups.Count;
            deckId = match.Groups[index - 1].Value;
            if (deckId != string.Empty) return true;
        }

        return false;
    }
}
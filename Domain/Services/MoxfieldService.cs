using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

using Domain.Clients;
using Domain.Factories;
using Domain.Models.DTO;
using Domain.Models.DTO.Moxfield;

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
        Console.WriteLine(JsonSerializer.Serialize(deckDto));
        if (deckDto is null)
        {
            _logger.LogError("Deck not loaded from internet");
            return null;
        }

        var deck = new DeckDetailsDTO {  };

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
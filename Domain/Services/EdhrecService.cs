using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

using Domain.Clients;
using Domain.Factories;
using Domain.Models.DTO;
using System.Net;

namespace Domain.Services;

public interface IEdhrecService : IDeckRetriever
{
    /// <summary>
    /// Tries to extract the deck ID from the given URL.
    /// </summary>
    /// <param name="url">The URL to extract the deck ID from.</param>
    /// <param name="deckId">The extracted deck ID, if successful.</param>
    /// <returns><c>true</c> if the deck ID was successfully extracted; otherwise, <c>false</c>.</returns>
    bool TryExtractDeckIdFromUrl(string url, out string deckId);
}

public class EdhrecService(IEdhrecClient edhrecClient, ILogger<EdhrecService> logger) : IEdhrecService
{
    private readonly IEdhrecClient _edhrecClient = edhrecClient;
    private readonly ILogger<EdhrecService> _logger = logger;

    public async Task<DeckDetailsDTO?> RetrieveDeckFromWeb(string deckUrl)
    {
        TryExtractDeckIdFromUrl(deckUrl, out string deckId);
        
        var deckHtml = await _edhrecClient.GetDeck(deckId);
        if (deckHtml is null)
        {
            _logger.LogError("Deck not loaded from internet");
            return null;
        }

        var deck = ScrapDeckFromHtml(deckHtml);

        return deck;
    }
    
    public bool TryExtractDeckIdFromUrl(string url, out string deckId)
    {
        deckId = string.Empty;
        string pattern = @"^https:\/\/(www\.)?edhrec\.com\/deckpreview\/([\w\-._~]+)\/?$";
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
    
    private DeckDetailsDTO ScrapDeckFromHtml(string htmlContent)
    {
        // Match deck name
        var deckNamePattern = "<h3.*?>(.*?)</h3>";
        var deckNameMatch = Regex.Match(htmlContent, deckNamePattern);
        string deckName = deckNameMatch.Success 
            ? deckNameMatch.Groups[1].Value.Trim() 
            : string.Empty;
        var deck = new DeckDetailsDTO { Name = deckName };
    
        // Match card entries in a URL
        string cardListPattern = @"(\d+\+[^&]+)";
        var matches = Regex.Matches(htmlContent, cardListPattern);

        foreach (Match match in matches)
        {
            if (match.Success)
            {
                string encodedCard = match.Groups[1].Value;
                string decodedCard = Uri.UnescapeDataString(encodedCard).Replace("+", " ");

                // Match card quantity and name
                var cardMatch = Regex.Match(decodedCard, @"(\d+)\s+(.+)");
                if (cardMatch.Success)
                {
                    int quantity = int.Parse(cardMatch.Groups[1].Value);
                    string name = cardMatch.Groups[2].Value.Trim(); // Trim to remove unwanted characters
                    deck.Cards.Add(new CardEntryDTO { Quantity = quantity, Name = name });
                }
            }
        }
    
        return deck;
    }
}
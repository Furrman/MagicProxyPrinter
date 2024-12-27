using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Web;

using HtmlAgilityPack;

using Domain.Clients;
using Domain.Factories;
using Domain.Models.DTO;

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
        var deck = new DeckDetailsDTO();
        
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);
        
        // Get the deck name
        var deckNameNode = htmlDoc.DocumentNode.SelectSingleNode("//h3[contains(text(), 'Deck with')]");
        if (deckNameNode != null)
        {
            deck.Name = HttpUtility.HtmlDecode(deckNameNode.InnerText.Trim());
        }
    
        // Get card entries
        var nodes = htmlDoc.DocumentNode.SelectNodes("//span[contains(@class, 'Card_name__')]");
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                string decodedName = HttpUtility.HtmlDecode(node.InnerText.Trim());
                deck.Cards.Add(new CardEntryDTO { Name = decodedName, Quantity = 1 });
                Console.WriteLine(decodedName);
            }
        }
        
        return deck;
    }
}
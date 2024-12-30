using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

using Domain.Clients;
using Domain.Factories;
using Domain.Models.DTO;

namespace Domain.Services;

public interface IGoldfishService : IDeckBuildService
{
    /// <summary>
    /// Tries to extract matching relative path from the given URL.
    /// </summary>
    /// <param name="url">The URL to extract the deck ID from.</param>
    /// <param name="deckId">Relative path of URL, if successful.</param>
    /// <returns><c>true</c> if the deck ID was successfully extracted; otherwise, <c>false</c>.</returns>
    bool TryExtractRelativePath(string url, out string deckId);
}

public class GoldfishService(IGoldfishClient goldfishClient,
    ILogger<GoldfishService> logger) : IGoldfishService
{
    private readonly IGoldfishClient _goldfishClient = goldfishClient;
    private readonly ILogger<GoldfishService> _logger = logger;

    public async Task<DeckDetailsDTO?> RetrieveDeckFromWeb(string deckUrl)
    {
        var htmlContent = await GetDeckHtmlContent(deckUrl);
        if (htmlContent is null)
        {
            _logger.LogError("Deck not loaded from internet");
            return null;
        }

        var deck = ScrapDeckFromHtml(htmlContent);

        return deck;
    }
    
    private DeckDetailsDTO ScrapDeckFromHtml(string htmlContent)
    {
        HtmlDocument htmlDoc = new();
        htmlDoc.LoadHtml(htmlContent);
        
        DeckDetailsDTO deck = new();
        
        // Extract the deck name
        var deckNameNode = htmlDoc.DocumentNode.SelectSingleNode("//h1[@class='title']");
        deck.Name = deckNameNode != null ? deckNameNode.InnerText.Trim() : string.Empty;

        // Extract cards
        var cardRows = htmlDoc.DocumentNode.SelectNodes("//table[contains(@class, 'deck-view-deck-table')]//tr[td]");

        if (cardRows != null)
        {
            foreach (var row in cardRows)
            {
                var quantityNode = row.SelectSingleNode("./td[contains(@class, 'text-right')]");
                var nameNode = row.SelectSingleNode("./td//a[contains(@data-card-id, '')]");

                if (quantityNode == null || nameNode == null)
                {
                    continue;
                }

                int.TryParse(quantityNode.InnerText.Trim(), out int quantity);
                if (quantity == 0)
                {
                    quantity = 1;
                }
                string cardName = nameNode.InnerText.Trim();
                string cardId = nameNode.GetAttributeValue("data-card-id", string.Empty);
                Guid? id = null;
                if (!string.IsNullOrEmpty(cardId))
                {
                    var result = Guid.TryParse(cardId, out Guid guid);
                    if (result)
                    {
                        id = guid;
                    }
                }
                    
                deck.Cards.Add(new CardEntryDTO { Id = id, Name = cardName, Quantity = quantity });
            }
        }

        return deck;
    }

    public bool TryExtractRelativePath(string url, out string relativePath)
    {
        relativePath = string.Empty;

        const string pattern = @"^https://(www\.)?mtggoldfish\.com/(?<relativePath>(deck|archetype)/.+)$";
        Regex regex = new(pattern);

        Match match = regex.Match(url);
        if (match.Success)
        {
            relativePath = match.Groups["relativePath"].Value;
            return true;
        }

        return false;
    }


    private async Task<string?> GetDeckHtmlContent(string deckUrl)
    {
        TryExtractRelativePath(deckUrl, out string relativePath);
        
        var htmlContent = await _goldfishClient.GetCardsInHtml(relativePath);
        if (htmlContent is null)
        {
            _logger.LogError("Deck not loaded from internet");
            return null;
        }

        return htmlContent;
    }
}
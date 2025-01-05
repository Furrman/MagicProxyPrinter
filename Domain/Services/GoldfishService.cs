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
        var deckInputNode = htmlDoc.DocumentNode.SelectSingleNode("//input[@id='deck_input_deck']");
        if (deckInputNode == null)
        {
            return deck;
        }
        string deckData = System.Net.WebUtility.HtmlDecode(deckInputNode.GetAttributeValue("value", string.Empty));
        var lines = deckData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            // Trim the line and skip if it's empty or doesn't start with a number
            var trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine) || !char.IsDigit(trimmedLine[0]))
                continue;

            var parts = trimmedLine.Split(' ', 2);
            if (parts.Length < 2) continue;

            if (!int.TryParse(parts[0], out int quantity))
                continue;

            string cardName = System.Net.WebUtility.HtmlDecode(parts[1].Trim());

            deck.Cards.Add(new CardEntryDTO
            {
                Id = null, // No GUID is provided in this case
                Name = cardName,
                Quantity = quantity,
            });
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
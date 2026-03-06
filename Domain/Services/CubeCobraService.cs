using Domain.Clients;
using Domain.Factories;
using Domain.Models.DTO;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Domain.Services;

public interface ICubeCobraService : IDeckBuildService
{
    /// <summary>
    /// Tries to extract matching relative path from the given URL.
    /// </summary>
    /// <param name="url">The URL to extract the deck ID from.</param>
    /// <param name="deckId">Relative path of URL, if successful.</param>
    /// <returns><c>true</c> if the deck ID was successfully extracted; otherwise, <c>false</c>.</returns>
    bool TryExtractRelativePath(string url, out Guid deckId);
}

public class CubeCobraService(
    ICubeCobraClient cubeCobraClient,
    ILogger<CubeCobraService> logger) : ICubeCobraService
{
    private readonly ICubeCobraClient _cubeCobraClient = cubeCobraClient;
    private readonly ILogger<CubeCobraService> _logger = logger;

    public async Task<DeckDetailsDTO?> RetrieveDeckFromWeb(string deckUrl)
    {
        var htmlContent = await GetDeckHtmlContent(deckUrl);
        if (string.IsNullOrWhiteSpace(htmlContent)) return null;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        var scriptNode = htmlDoc.DocumentNode
            .SelectNodes("//script")
            ?.FirstOrDefault(n => n.InnerText.Contains("window.reactProps"));

        if (scriptNode == null) return null;

        var match = Regex.Match(
            scriptNode.InnerText,
            @"window\.reactProps\s*=\s*(\{.*\})\s*;",
            RegexOptions.Singleline);

        if (!match.Success) return null;

        var deck = new DeckDetailsDTO();
        var json = match.Groups[1].Value;

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        deck.Name = root
            .GetProperty("cube")
            .GetProperty("name")
            .GetString() ?? string.Empty;

        var cardsNode = root.GetProperty("cards");
        var mainboard = cardsNode
            .GetProperty("mainboard")
            .EnumerateArray();
        var maybeboard = cardsNode.TryGetProperty("maybeboard", out var maybe)
            ? maybe.EnumerateArray()
            : Enumerable.Empty<JsonElement>();
        var allCards = mainboard.Concat(maybeboard);
        
        var groupedCards = allCards
            .GroupBy(card => card.GetProperty("name").GetString());
        foreach (var group in groupedCards)
        {
            var first = group.First();
            var card = new CardEntryDTO
            {
                Id = first.TryGetProperty("cardID", out var cardId)
                    ? Guid.Parse(cardId.ToString())
                    : null,
                Name = group.Key ?? string.Empty,
                Quantity = group.Count(),
                ExpansionCode = first.TryGetProperty("set", out var set)
                    ? set.GetString()
                    : null,
                CollectorNumber = first.TryGetProperty("collector_number", out var cn)
                    ? cn.GetString()
                    : null,
                Foil = first.TryGetProperty("finish", out var finish)
                       && finish.GetString()?.Contains("foil", StringComparison.OrdinalIgnoreCase) == true,
                Etched = first.TryGetProperty("finish", out var etched)
                         && etched.GetString()?.Contains("etched", StringComparison.OrdinalIgnoreCase) == true
            }; 
            
            deck.Cards.Add(card);
        }

        return deck;
    }

    public bool TryExtractRelativePath(string url, out Guid deckId)
    {
        deckId = Guid.Empty;

        const string pattern =
            @"^(https://)?(www\.)?cubecobra\.com/cube/list/(?<deckId>[0-9a-fA-F-]{36})/?$";
        var regex = new Regex(pattern);

        Match match = regex.Match(url);
        if (!match.Success) return false;

        var stringifyDeckId = match.Groups["deckId"].Value;
        var parseResult = Guid.TryParse(stringifyDeckId, out deckId);
        return parseResult;
    }
    
    
    private async Task<string?> GetDeckHtmlContent(string deckUrl)
    {
        if (!TryExtractRelativePath(deckUrl, out Guid deckId))
        {
            _logger.LogError("Error parsing deckId from url");
            return null;
        }
        
        var htmlContent = await _cubeCobraClient.GetCardsInHtml(deckId.ToString());
        if (htmlContent is null)
        {
            _logger.LogError("Deck not loaded from internet");
            return null;
        }

        return htmlContent;
    }
}
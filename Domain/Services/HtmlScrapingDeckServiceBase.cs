using Microsoft.Extensions.Logging;

using Domain.Clients;
using Domain.Models.DTO;

namespace Domain.Services;

/// <summary>
/// Shared extract-path/fetch/scrape skeleton for services whose site has no JSON API and must be
/// scraped from HTML or other embedded content (EDHRec, Goldfish, CubeCobra).
/// </summary>
/// <typeparam name="TSelf">The concrete derived service type, used for the logger category.</typeparam>
/// <typeparam name="TClient">The site's HTML-fetching client.</typeparam>
/// <typeparam name="TPath">
/// The value extracted from the deck URL and sent to the client (e.g. a relative path string, or a
/// cube GUID). Sent via its <see cref="object.ToString"/> representation.
/// </typeparam>
public abstract class HtmlScrapingDeckServiceBase<TSelf, TClient, TPath>(TClient client, ILogger<TSelf> logger)
    where TClient : IHtmlScrapingClient
{
    public async Task<DeckDetailsDTO?> RetrieveDeckFromWeb(string deckUrl)
    {
        var htmlContent = await GetDeckHtmlContent(deckUrl);
        return htmlContent is null ? null : ScrapDeckFromHtml(htmlContent);
    }

    public abstract bool TryExtractRelativePath(string url, out TPath path);

    public abstract DeckDetailsDTO? ScrapDeckFromHtml(string htmlContent);

    protected async Task<string?> GetDeckHtmlContent(string deckUrl)
    {
        if (!TryExtractRelativePath(deckUrl, out var path))
        {
            logger.LogError("Error parsing relative path from url");
            return null;
        }

        var htmlContent = await client.GetCardsInHtml(path?.ToString() ?? string.Empty);
        if (htmlContent is null)
        {
            logger.LogError("Deck not loaded from internet");
            return null;
        }

        return htmlContent;
    }
}

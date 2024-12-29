using Microsoft.Extensions.Logging;

using Domain.Factories;
using Domain.Models.DTO;
using Domain.Services;

namespace Domain.Strategies;

/// <summary>
/// Class using strategy pattern to retrieve deck from specific website depending on URL.
/// </summary>
public interface IDeckRetrieveStrategy
{
    Task<DeckDetailsDTO?> GetDeck(string deckUrl);
}

public class DeckRetrieveStrategy(IServiceFactory factory, ILogger<DeckRetrieveStrategy> logger) 
    : IDeckRetrieveStrategy
{
    private readonly IServiceFactory _serviceFactory = factory;
    private readonly ILogger<DeckRetrieveStrategy> _logger = logger;

    public async Task<DeckDetailsDTO?> GetDeck(string deckUrl)
    {
        var service = _serviceFactory.GetDeckBuildService(deckUrl);
        if (service == null)
        {
            return null;
        }

        // Handle case when link to deck is stored in EDHRec page
        if (service is IEdhrecService edhrecService)
        {
            var (originalDeckLink, htmlContent) = await edhrecService.GetOriginalDeckLink(deckUrl);
            if (originalDeckLink is not null)
            {
                var serviceToOriginalDeck = _serviceFactory.GetDeckBuildService(originalDeckLink);
                if (serviceToOriginalDeck is not null)
                {
                    return await serviceToOriginalDeck.RetrieveDeckFromWeb(originalDeckLink);
                }

                _logger.LogWarning("Could not get deck from link {originalDeckLink} retrieved from {deckUrl}",
                    originalDeckLink, deckUrl);
            }

            return htmlContent is not null ? edhrecService.ScrapDeckFromHtml(htmlContent) : null;
        }

        return await service.RetrieveDeckFromWeb(deckUrl);
    }
}
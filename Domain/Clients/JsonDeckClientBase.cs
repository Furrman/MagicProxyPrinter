using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Domain.Clients;

/// <summary>
/// Shared GET/log/error-handling skeleton for clients that retrieve a deck as JSON from a site's
/// own API (Archidekt, Moxfield).
/// </summary>
public abstract class JsonDeckClientBase<TClient, TDeckId, TDto>(HttpClient httpClient, ILogger<TClient> logger)
    where TDto : class
{
    public async Task<TDto?> GetDeck(TDeckId deckId)
    {
        TDto? deckDto = null;
        var requestUrl = BuildRequestUrl(deckId);
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(requestUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DeckId: {deckId} Error in getting card list from the deck", deckId);
            return deckDto;
        }

        if (response.IsSuccessStatusCode)
        {
            try
            {
                deckDto = await response.Content.ReadFromJsonAsync<TDto>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DeckId: {deckId} Error in parsing card list from the deck", deckId);
                return null;
            }
        }
        else
        {
            logger.LogWarning("DeckId: {deckId} Failure response from getting card list from the deck Request: {statusCode} {reasonPhrase}", deckId, response.StatusCode, response.ReasonPhrase);
        }

        return deckDto;
    }

    /// <summary>
    /// Builds the request URL from the deck ID.
    /// </summary>
    protected abstract string BuildRequestUrl(TDeckId deckId);
}

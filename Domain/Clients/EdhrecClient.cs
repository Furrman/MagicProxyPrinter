using Microsoft.Extensions.Logging;

namespace Domain.Clients;

/// <summary>
/// Represents an interface for interacting with the EDHRec portal.
/// </summary>
public interface IEdhrecClient
{
    /// <summary>
    /// Retrieves a deck from the EDHRec portal based on the specified deck ID.
    /// </summary>
    /// <param name="deckId">The ID of the deck to retrieve.</param>
    /// <returns>Html website with deck details if request was successful, or <c>null</c> if the deck was not found.</returns>
    Task<string?> GetDeck(string deckId);
}

public class EdhrecClient(HttpClient httpClient, ILogger<EdhrecClient> logger) : IEdhrecClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<EdhrecClient> _logger = logger;

    public async Task<string?> GetDeck(string deckId)
    {
        var requestUrl = $"deckpreview/{deckId}";
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(requestUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeckId: {deckId} Error in getting card list from the deck", deckId);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("DeckId: {deckId} Failure response from getting card list from the deck Request: {statusCode} {reasonPhrase}", 
                deckId, response.StatusCode, response.ReasonPhrase);
            return null;
        }
        
        try
        {
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeckId: {deckId} Error in parsing card list from the deck", deckId);
            return null;
        }
    }
}

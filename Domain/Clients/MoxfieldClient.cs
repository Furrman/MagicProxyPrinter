using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

using Domain.Models.DTO.Moxfield;

namespace Domain.Clients;

/// <summary>
/// Represents an interface for interacting with the Archidekt API.
/// </summary>
public interface IMoxfieldClient
{
    /// <summary>
    /// Retrieves a deck from the Moxfield API based on the specified deck ID.
    /// </summary>
    /// <param name="deckId">The ID of the deck to retrieve.</param>
    /// <returns>Deck details if request was successful, or <c>null</c> if the deck was not found.</returns>
    Task<DeckDTO?> GetDeck(string deckId);
}

public class MoxfieldClient(HttpClient httpClient, ILogger<MoxfieldClient> logger) : IMoxfieldClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<MoxfieldClient> _logger = logger;

    public async Task<DeckDTO?> GetDeck(string deckId)
    {
        DeckDTO? deckDto = null;
        var requestUrl = $"decks/all/{deckId}";
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(requestUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeckId: {deckId} Error in getting card list from the deck", deckId);
            return deckDto;
        }

        if (response.IsSuccessStatusCode)
        {
            try
            {
                deckDto = await response.Content.ReadFromJsonAsync<DeckDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeckId: {deckId} Error in parsing card list from the deck", deckId);
                return null;
            }
        }
        else
        {
            _logger.LogWarning("DeckId: {deckId} Failure response from getting card list from the deck Request: {statusCode} {reasonPhrase}", deckId, response.StatusCode, response.ReasonPhrase);
        }

        return deckDto;
    }
}

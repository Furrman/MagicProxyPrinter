using Microsoft.Extensions.Logging;

using Domain.Models.DTO.Moxfield;

namespace Domain.Clients;

/// <summary>
/// Represents an interface for interacting with the Moxfield API.
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

public class MoxfieldClient(HttpClient httpClient, ILogger<MoxfieldClient> logger)
    : JsonDeckClientBase<MoxfieldClient, string, DeckDTO>(httpClient, logger), IMoxfieldClient
{
    protected override string BuildRequestUrl(string deckId) => $"decks/all/{deckId}";
}

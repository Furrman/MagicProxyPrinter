using Microsoft.Extensions.Logging;

using Domain.Models.DTO.Archidekt;

namespace Domain.Clients;

/// <summary>
/// Represents an interface for interacting with the Archidekt API.
/// </summary>
public interface IArchidektClient
{
    /// <summary>
    /// Retrieves a deck from the Archidekt API based on the specified deck ID.
    /// </summary>
    /// <param name="deckId">The ID of the deck to retrieve.</param>
    /// <returns>Deck details if request was successful, or <c>null</c> if the deck was not found.</returns>
    Task<DeckDTO?> GetDeck(int deckId);
}

public class ArchidektClient(HttpClient httpClient, ILogger<ArchidektClient> logger)
    : JsonDeckClientBase<ArchidektClient, int, DeckDTO>(httpClient, logger), IArchidektClient
{
    protected override string BuildRequestUrl(int deckId) => $"decks/{deckId}/";
}

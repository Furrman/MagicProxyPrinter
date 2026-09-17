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
    /// <param name="relativePath">Relative path of URL.</param>
    /// <returns>Html website with deck details if request was successful, or <c>null</c> if the deck was not found.</returns>
    Task<string?> GetCardsInHtml(string relativePath);
}

public class EdhrecClient(HttpClient httpClient, ILogger<EdhrecClient> logger)
    : HtmlScrapingClientBase<EdhrecClient>(httpClient, logger), IEdhrecClient;

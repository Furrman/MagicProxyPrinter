using Microsoft.Extensions.Logging;

namespace Domain.Clients;

/// <summary>
/// Represents an interface for interacting with the Goldfish portal.
/// </summary>
public interface IGoldfishClient
{
    /// <summary>
    /// Retrieves a deck from the Goldfish portal based on the specified deck ID.
    /// </summary>
    /// <param name="relativePath">Relative path of URL.</param>
    /// <returns>Html website with deck details if request was successful, or <c>null</c> if the deck was not found.</returns>
    Task<string?> GetCardsInHtml(string relativePath);
}

public class GoldfishClient(HttpClient httpClient, ILogger<GoldfishClient> logger)
    : HtmlScrapingClientBase<GoldfishClient>(httpClient, logger), IGoldfishClient;

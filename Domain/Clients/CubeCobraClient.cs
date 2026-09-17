using Microsoft.Extensions.Logging;

namespace Domain.Clients;

/// <summary>
/// Represents an interface for interacting with the CubeCobra portal.
/// </summary>
public interface ICubeCobraClient
{
    /// <summary>
    /// Retrieves a deck from the CubeCobra portal based on the specified deck ID.
    /// </summary>
    /// <param name="relativePath">Relative path of URL.</param>
    /// <returns>Html website with deck details if request was successful, or <c>null</c> if the deck was not found.</returns>
    Task<string?> GetCardsInHtml(string relativePath);
}

public class CubeCobraClient(HttpClient httpClient, ILogger<CubeCobraClient> logger)
    : HtmlScrapingClientBase<CubeCobraClient>(httpClient, logger), ICubeCobraClient
{
    protected override string BuildRequestUri(string relativePath) => $"/cube/list/{relativePath}";
}

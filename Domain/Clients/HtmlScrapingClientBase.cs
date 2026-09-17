using Microsoft.Extensions.Logging;

namespace Domain.Clients;

/// <summary>
/// Shared GET/log/error-handling skeleton for clients that retrieve a deck as raw HTML from a
/// site with no JSON API (EDHRec, Goldfish, CubeCobra).
/// </summary>
public abstract class HtmlScrapingClientBase<TClient>(HttpClient httpClient, ILogger<TClient> logger)
{
    public async Task<string?> GetCardsInHtml(string relativePath)
    {
        var requestUri = BuildRequestUri(relativePath);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(requestUri);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RelativePath: {relativePath} Error in getting card list from the deck",
                relativePath);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("RelativePath: {relativePath} Failure response from getting card list from the deck Request: {statusCode} {reasonPhrase}",
                relativePath, response.StatusCode, response.ReasonPhrase);
            return null;
        }

        try
        {
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RelativePath: {relativePath} Error in parsing card list from the deck",
                relativePath);
            return null;
        }
    }

    /// <summary>
    /// Builds the request URI from the relative path. Override when the site needs a path prefix.
    /// </summary>
    protected virtual string BuildRequestUri(string relativePath) => relativePath;
}

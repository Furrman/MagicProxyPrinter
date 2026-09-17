namespace Domain.Clients;

/// <summary>
/// Common shape shared by clients that fetch a deck as raw HTML/embedded content from a site with
/// no JSON API, letting <see cref="Domain.Services.HtmlScrapingDeckServiceBase{TSelf,TClient,TPath}"/>
/// call any of them without knowing which site it is.
/// </summary>
public interface IHtmlScrapingClient
{
    /// <summary>
    /// Retrieves the raw HTML/content for a deck at the given relative path.
    /// </summary>
    /// <param name="relativePath">Relative path of URL.</param>
    /// <returns>The content if the request was successful, or <c>null</c> otherwise.</returns>
    Task<string?> GetCardsInHtml(string relativePath);
}

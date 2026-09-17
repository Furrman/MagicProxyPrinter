using Microsoft.Extensions.Logging;

namespace Domain.Clients;

/// <summary>
/// Represents an interface for interacting with the CubeCobra portal.
/// </summary>
public interface ICubeCobraClient : IHtmlScrapingClient;

public class CubeCobraClient(HttpClient httpClient, ILogger<CubeCobraClient> logger)
    : HtmlScrapingClientBase<CubeCobraClient>(httpClient, logger), ICubeCobraClient
{
    protected override string BuildRequestUri(string relativePath) => $"/cube/list/{relativePath}";
}

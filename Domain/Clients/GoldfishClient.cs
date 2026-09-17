using Microsoft.Extensions.Logging;

namespace Domain.Clients;

/// <summary>
/// Represents an interface for interacting with the Goldfish portal.
/// </summary>
public interface IGoldfishClient : IHtmlScrapingClient;

public class GoldfishClient(HttpClient httpClient, ILogger<GoldfishClient> logger)
    : HtmlScrapingClientBase<GoldfishClient>(httpClient, logger), IGoldfishClient;

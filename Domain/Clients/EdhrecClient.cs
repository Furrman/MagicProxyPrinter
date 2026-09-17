using Microsoft.Extensions.Logging;

namespace Domain.Clients;

/// <summary>
/// Represents an interface for interacting with the EDHRec portal.
/// </summary>
public interface IEdhrecClient : IHtmlScrapingClient;

public class EdhrecClient(HttpClient httpClient, ILogger<EdhrecClient> logger)
    : HtmlScrapingClientBase<EdhrecClient>(httpClient, logger), IEdhrecClient;

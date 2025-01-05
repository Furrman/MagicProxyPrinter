using Domain.Constants;
using Domain.Models.DTO;
using Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Factories;

/// <summary>
/// Interface to retrieve a deck from web.
/// </summary>
public interface IDeckBuildService
{
    /// <summary>
    /// Retrieve a deck from web.
    /// </summary>
    /// <param name="deckUrl">Url to the deck available online.</param>
    /// <returns>Deck with all details or null if error occurs.</returns>
    Task<DeckDetailsDTO?> RetrieveDeckFromWeb(string deckUrl);
}

/// <summary>
/// Factory for retrieving correct service for obtaining a deck from web.
/// </summary>
public interface IServiceFactory
{
    /// <summary>
    /// Get the correct service for obtaining a deck from web.
    /// </summary>
    /// <param name="deckUrl">Url to the deck available online.</param>
    /// <returns>Object able to retrieve deck from web or null if no website is matching url.</returns>
    IDeckBuildService? GetDeckBuildService(string deckUrl);
}

public class ServiceFactory(IServiceProvider serviceProvider) : IServiceFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public IDeckBuildService? GetDeckBuildService(string deckUrl)
    {
        var domain = GetDomainFromUrl(deckUrl);
        return domain switch
        {
            DeckBuilders.ARCHIDEKT_URL_DOMAIN => _serviceProvider.GetService<IArchidektService>(),
            DeckBuilders.EDHREC_URL_DOMAIN => _serviceProvider.GetService<IEdhrecService>(),
            DeckBuilders.GOLDFISH_URL_DOMAIN => _serviceProvider.GetService<IGoldfishService>(),
            DeckBuilders.MOXFIELD_URL_DOMAIN => _serviceProvider.GetService<IMoxfieldService>(),
            _ => null
        };
    }

    private string GetDomainFromUrl(string url)
    {
        try
        {
            // Try to create a Uri object; if it fails without a scheme, prepend "https://"
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            {
                if (!Uri.TryCreate("https://" + url, UriKind.Absolute, out uri))
                {
                    return string.Empty;
                }
            }
            
            string host = uri.Host.ToLowerInvariant();
            if (host.StartsWith("www."))
            {
                host = host[4..];
            }

            return host;
        }
        catch
        {
            return string.Empty;
        }
    }
}
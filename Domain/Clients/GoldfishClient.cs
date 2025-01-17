﻿using Microsoft.Extensions.Logging;

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

public class GoldfishClient(HttpClient httpClient, ILogger<GoldfishClient> logger) : IGoldfishClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<GoldfishClient> _logger = logger;

    public async Task<string?> GetCardsInHtml(string relativePath)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(relativePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RelativePath: {relativePath} Error in getting card list from the deck", 
                relativePath);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("RelativePath: {relativePath} Failure response from getting card list from the deck Request: {statusCode} {reasonPhrase}", 
                relativePath, response.StatusCode, response.ReasonPhrase);
            return null;
        }
        
        try
        {
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RelativePath: {relativePath} Error in parsing card list from the deck", 
                relativePath);
            return null;
        }
    }
}

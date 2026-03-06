using Domain.Factories;
using Domain.Models.DTO;
using System.Text.RegularExpressions;

namespace Domain.Services;

public interface ICubeCobraService : IDeckBuildService
{
    /// <summary>
    /// Tries to extract matching relative path from the given URL.
    /// </summary>
    /// <param name="url">The URL to extract the deck ID from.</param>
    /// <param name="deckId">Relative path of URL, if successful.</param>
    /// <returns><c>true</c> if the deck ID was successfully extracted; otherwise, <c>false</c>.</returns>
    bool TryExtractRelativePath(string url, out Guid deckId);
}

public class CubeCobraService : ICubeCobraService
{
    public Task<DeckDetailsDTO?> RetrieveDeckFromWeb(string deckUrl)
    {
        throw new NotImplementedException();
    }

    public bool TryExtractRelativePath(string url, out Guid deckId)
    {
        deckId = Guid.Empty;

        const string pattern =
            @"^(https://)?(www\.)?cubecobra\.com/cube/list/(?<deckId>[0-9a-fA-F-]{36})/?$";
        var regex = new Regex(pattern);

        Match match = regex.Match(url);
        if (!match.Success) return false;

        var stringifyDeckId = match.Groups["deckId"].Value;
        var parseResult = Guid.TryParse(stringifyDeckId, out deckId);
        return parseResult;
    }
}
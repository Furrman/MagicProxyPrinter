using Domain.Services;

namespace ConsoleApp.Validation;

/// <summary>
/// Validates CLI arguments before a deck generation run starts.
/// </summary>
internal static class InputValidator
{
    private const string MissingInputMessage = """
You have to provide at least one from this list:
                - path to exported deck
                - url to your deck.

                Use --help to see more information.
""";

    /// <summary>
    /// Validates the CLI arguments, stopping at the first failing rule (mirrors the original
    /// inline checks in Program.cs, which returned as soon as one rule failed).
    /// </summary>
    /// <returns>The error messages to display, in display order, or an empty list if the arguments are valid.</returns>
    public static IReadOnlyList<string> Validate(string? deckFilePath, string? deckUrl, string? languageCode, int? tokenCopies, ILanguageService languageService)
    {
        if (deckFilePath is null && deckUrl is null)
        {
            return [MissingInputMessage];
        }

        if (languageCode is not null && !languageService.IsValidLanguage(languageCode))
        {
            return ["You have to specify correct language code.", $"Language codes: {languageService.AvailableLanguages}"];
        }

        if (tokenCopies <= 0)
        {
            return ["Number of copies for each token has to be greater than 0."];
        }

        if (tokenCopies > 100)
        {
            return ["Number of copies for each token has to be less than 100."];
        }

        return [];
    }
}

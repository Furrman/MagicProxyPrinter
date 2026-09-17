using Microsoft.Extensions.DependencyInjection;
using CoconoaApp = Cocona.CoconaLiteApp;
using CoconoaOptions = Cocona.OptionAttribute;
using Domain;
using Domain.Services;
using Domain.Models.Events;
using ConsoleApp.Configuration;
using ConsoleApp.Helpers;
using ConsoleApp.Validation;

namespace ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        using var serviceProvider = DependencyInjectionConfigurator.Setup();

        CoconoaApp.Run(([CoconoaOptions(Description = "Filepath to exported deck")] string? deckFilePath,
            [CoconoaOptions(Description = "URL link to deck")]string? deckUrl,
            [CoconoaOptions(Description = "Set language for all cards to print")] string? languageCode = null,
            [CoconoaOptions(Description = "Number of copy for each token")] int? tokenCopies = null,
            [CoconoaOptions(Description = "Group tokens based on the name")] bool groupTokens = false,
            [CoconoaOptions(Description = "Directory path to output file(s)")]string? outputPath = null,
            [CoconoaOptions(Description = "Filename of the output word file")]string? outputFileName = null,
            [CoconoaOptions(Description = "Include emblems attached to a cards into output document")] bool includeEmblems = false,
            [CoconoaOptions(Description = "Flag to store original images in the same folder as output file")] bool storeOriginalImages = false) =>
        {
            // Domain services are registered Scoped; an explicit scope per run makes that lifetime
            // meaningful and ensures scoped disposables (e.g. IWordDocumentWrapper) are disposed
            // as soon as this run finishes, rather than only when the root provider disposes.
            using var scope = serviceProvider.CreateScope();
            var scopedProvider = scope.ServiceProvider;

            var languageService = scopedProvider.GetService<ILanguageService>()!;
            var validationErrors = InputValidator.Validate(deckFilePath, deckUrl, languageCode, tokenCopies, languageService);
            if (validationErrors.Count > 0)
            {
                foreach (var error in validationErrors)
                {
                    ConsoleUtility.WriteErrorMessage(error);
                }
                return -1;
            }

            var magicProxyPrinter = scopedProvider.GetService<IMagicProxyPrinter>()!;
            magicProxyPrinter.ProgressUpdate += UpdateProgressOnConsole;
            magicProxyPrinter.GenerateWord(deckUrl,
                deckFilePath,
                outputPath,
                outputFileName,
                languageCode,
                tokenCopies ?? 0,
                groupTokens,
                includeEmblems,
                storeOriginalImages).Wait();
            return 0;
        });
    }

    private static void UpdateProgressOnConsole(object? sender, UpdateProgressEventArgs e)
    {
        if (e.Percent is not null)
        {
            if (e.Percent == 0)
            {
                var stageInfo = e.Stage switch
                {
                    CreateMagicDeckDocumentStageEnum.GetDeckDetails => "(1/3) Get deck details",
                    CreateMagicDeckDocumentStageEnum.DownloadImages => "(2/3) Download images",
                    CreateMagicDeckDocumentStageEnum.GenerateDocument => "(3/3) Generate document",
                    _ => string.Empty
                };
                ConsoleUtility.WriteInNewLine(stageInfo);
            }
            ConsoleUtility.WriteProgressBar((int)e.Percent, true);
        }

        if (e.ErrorMessage is not null)
        {
            ConsoleUtility.WriteErrorMessage(e.ErrorMessage);
        }
    }
}
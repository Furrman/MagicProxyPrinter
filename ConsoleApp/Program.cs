﻿using Microsoft.Extensions.DependencyInjection;

using CoconoaApp = Cocona.CoconaLiteApp;
using CoconoaOptions = Cocona.OptionAttribute;

using Domain;
using Domain.Services;
using Domain.Models.Events;

using ConsoleApp.Configuration;
using ConsoleApp.Helpers;

namespace ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        var serviceProvider = DependencyInjectionConfigurator.Setup();

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
            if (deckUrl is null && deckUrl is null)
            {
                ConsoleUtility.WriteErrorMessage(@"You have to provide at least one from this list:
                - path to exported deck
                - url to your deck.
                
                Use --help to see more information.");
                return;
            }

            var languageService = serviceProvider.GetService<ILanguageService>()!;
            if (languageCode is not null && languageService.IsValidLanguage(languageCode) == false)
            {
                ConsoleUtility.WriteErrorMessage("You have to specify correct language code.");
                ConsoleUtility.WriteErrorMessage($"Language codes: {languageService.AvailableLanguages}");
                return;
            }

            if (tokenCopies <= 0)
            {
                ConsoleUtility.WriteErrorMessage("Number of copies for each token has to be greater than 0.");
                return;
            }
            if (tokenCopies > 100)
            {
                ConsoleUtility.WriteErrorMessage("Number of copies for each token has to be less than 100.");
                return;
            }

            var archidektPrinter = serviceProvider.GetService<IMagicProxyPrinter>()!;
            archidektPrinter.ProgressUpdate += UpdateProgressOnConsole;
            archidektPrinter.GenerateWord(deckUrl, 
                deckFilePath, 
                outputPath, 
                outputFileName, 
                languageCode,
                tokenCopies ?? 0,
                groupTokens,
                includeEmblems,
                storeOriginalImages).Wait();
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
                    CreateMagicDeckDocumentStageEnum.GetDeckDetails => "(1/2) Get deck details",
                    CreateMagicDeckDocumentStageEnum.SaveToDocument => "(2/2) Download images",
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
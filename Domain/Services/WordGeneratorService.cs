using Microsoft.Extensions.Logging;
using DocumentFormat.OpenXml.Wordprocessing;
using OfficeIMO.Word;
using Domain.Constants;
using Domain.Clients;
using Domain.Models.DTO;
using Domain.Models.Events;
using Domain.IO;

namespace Domain.Services;

/// <summary>
/// Represents a service for generating Word documents based on deck details.
/// </summary>
public interface IWordGeneratorService
{
    /// <summary>
    /// Event that is raised to report the progress of the images download.
    /// </summary>
    event EventHandler<DownloadImagesProgressEventArgs>? DownloadImagesProgress;
    /// <summary>
    /// Event that is raised to report the progress of the word generation.
    /// </summary>
    event EventHandler<GenerateDocumentProgressEventArgs>? GenerateWordProgress;

    /// <summary>
    /// Generates a Word document based on the provided deck details.
    /// </summary>
    /// <param name="deck">The deck details.</param>
    /// <param name="wordFileName">The file name of the Word document without extension.</param>
    /// <param name="outputFolder">The output folder where the Word document and optional images will be saved.</param>
    /// <param name="saveImages">A flag indicating whether to save images in the Word document.</param>
    /// <returns>A task representing the asynchronous generation of the Word document.</returns>
    Task GenerateWord(DeckDetailsDTO deck, string? wordFileName = null, string? outputFolder = null, bool saveImages = false);
}

public class WordGeneratorService(ILogger<WordGeneratorService> logger, IScryfallClient scryfallClient, IWordDocumentWrapper wordDocumentWrapper, IFileManager fileManager)
    : IWordGeneratorService
{
    public event EventHandler<DownloadImagesProgressEventArgs>? DownloadImagesProgress;
    public event EventHandler<GenerateDocumentProgressEventArgs>? GenerateWordProgress;

    private readonly ILogger<WordGeneratorService> _logger = logger;
    private readonly IScryfallClient _scryfallClient = scryfallClient;
    private readonly IWordDocumentWrapper _wordDocumentWrapper = wordDocumentWrapper;
    private readonly IFileManager _fileManager = fileManager;


    public async Task GenerateWord(DeckDetailsDTO deck, string? wordFileName = null, string? outputFolderDir = null, bool saveImages = false)
    {
        // Prepare
        int count = deck.Cards.SelectMany(c => c.CardSides).Count();
        if (count == 0)
        {
            RaiseDownloadImageError("No cards found in the deck");
            return;
        }
        
        var outputFolderPath = _fileManager.CreateOutputFolder(outputFolderDir);
        if (outputFolderPath is null)
        {
            RaiseGenerateWordError("Error in creating output folder");
            return;
        }

        var wordFilePath = _fileManager.ReturnCorrectWordFilePath(outputFolderPath, wordFileName ?? deck.Name);
        using WordDocument document = _wordDocumentWrapper.Create(wordFilePath);
        
        // Download images
        try
        {   
            await DownloadImages(document, deck, saveImages, outputFolderPath, count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in downloading images");
            RaiseDownloadImageError("Error in downloading images");
            return;
        }

        // Save word
        try
        {
            StartGeneratingWord();
            _wordDocumentWrapper.Save();
            EndGeneratingWord();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in saving Word file");
            RaiseDownloadImageError("Error in generating word");
            return;
        }
    }

    private async Task DownloadImages(WordDocument document,
        DeckDetailsDTO deck,
        bool saveImages,
        string outputFolderPath,
        int count)
    {
        _wordDocumentWrapper.SetMargins(WordMargin.Narrow);
        _wordDocumentWrapper.SetOrientation(PageOrientationValues.Landscape);
        _wordDocumentWrapper.SetPageSize(WordPageSize.A4);
        var paragraph = _wordDocumentWrapper.AddParagraph();

        int step = UpdateDownloadImageStep(0, count);
        foreach (var (card, side) in deck.Cards.SelectMany(
                     card => card.CardSides.Select(side => (card, side))))
        {
            var imageContent = await _scryfallClient.DownloadImage(side.ImageUrl);
            if (imageContent == null)
            {
                step = UpdateDownloadImageStep(step, count);
                continue;
            }

            if (saveImages)
            {
                await _fileManager.CreateImageFile(imageContent, outputFolderPath, side.Name);
            }

            AddImageToWord(paragraph, side.Name, imageContent, card.Quantity);

            step = UpdateDownloadImageStep(step, count);
        }
    }

    private void AddImageToWord(WordParagraph paragraph, string imageName, byte[] imageContent, int quantity)
    {
        try
        {
            for (int i = 0; i < quantity; i++)
            {
                _wordDocumentWrapper.AddImage(paragraph, imageContent, $"{imageName}{i}", width: CardDetails.CARD_WIDTH_PIXELS, height: CardDetails.CARD_HEIGHT_PIXELS);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error in adding image to word file - ImageName: {ImageName}",
                imageName);
        }
    }


    private void RaiseDownloadImageError(string errorMessage) =>
        DownloadImagesProgress?.Invoke(this,
            new DownloadImagesProgressEventArgs(percent: 100, errorMessage: errorMessage));
    private int UpdateDownloadImageStep(int step, int count)
    {
        var progressPercent = (double)step / count * 100;
        DownloadImagesProgress?.Invoke(this, new DownloadImagesProgressEventArgs(percent: progressPercent));
        return ++step;
    }

    private void RaiseGenerateWordError(string errorMessage) =>
        GenerateWordProgress?.Invoke(this, new(percent: 100, errorMessage: errorMessage));
    private void StartGeneratingWord() => GenerateWordProgress?.Invoke(this, new (percent: 0));
    private void EndGeneratingWord() => GenerateWordProgress?.Invoke(this, new (percent: 100));
}

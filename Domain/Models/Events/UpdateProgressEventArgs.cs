namespace Domain.Models.Events;

public class UpdateProgressEventArgs : EventArgs
{
    public CreateMagicDeckDocumentStageEnum Stage { get; init; }
    public double? Percent { get; init; }
    public string? ErrorMessage { get; init; }
}

public enum CreateMagicDeckDocumentStageEnum
{
    GetDeckDetails,
    DownloadImages,
    GenerateDocument
}
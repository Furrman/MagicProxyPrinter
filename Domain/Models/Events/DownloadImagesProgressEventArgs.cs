namespace Domain.Models.Events;

public class DownloadImagesProgressEventArgs : EventArgs
{
    public DownloadImagesProgressEventArgs()
    {
    }

    public DownloadImagesProgressEventArgs(double? percent, string? errorMessage)
    {
        Percent = percent;
        ErrorMessage = errorMessage;
    }

    public DownloadImagesProgressEventArgs(double? percent) : this()
    {
        Percent = percent;
    }

    public double? Percent { get; init; }
    public string? ErrorMessage { get; init; }
}

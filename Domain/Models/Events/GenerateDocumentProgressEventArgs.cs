namespace Domain.Models.Events;

public class GenerateDocumentProgressEventArgs : EventArgs
{
    public GenerateDocumentProgressEventArgs()
    {
    }

    public GenerateDocumentProgressEventArgs(double? percent)
    {
        Percent = percent;
    }

    public GenerateDocumentProgressEventArgs(double? percent, string? errorMessage) : this()
    {
        Percent = percent;
        ErrorMessage = errorMessage;
    }

    public double? Percent { get; init; }
    public string? ErrorMessage { get; init; }
}

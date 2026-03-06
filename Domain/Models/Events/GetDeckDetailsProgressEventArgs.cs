namespace Domain.Models.Events;

public class GetDeckDetailsProgressEventArgs : EventArgs
{
    public double? Percent { get; init; }
    public string? ErrorMessage { get; init; }
}

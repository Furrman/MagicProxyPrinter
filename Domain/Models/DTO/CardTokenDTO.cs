namespace Domain.Models.DTO;

public class CardTokenDTO
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Uri { get; set; }
    public bool IsEmblem { get; set; }
}
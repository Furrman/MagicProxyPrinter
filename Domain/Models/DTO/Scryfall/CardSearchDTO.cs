using System.Text.Json.Serialization;

namespace Domain.Models.DTO.Scryfall;

public record CardSearchDTO
{
    public ICollection<CardDataDTO?>? Data { get; set; }
}

public record CardDataDTO
{
    public string? Name { get; set; }
    public string? Lang { get; set; }
    public string? Set { get; set; }
    [JsonPropertyName("tcgplayer_etched_id")]
    public int? TcgPlayerEtchedId { get; set; }
    [JsonPropertyName("all_parts")]
    public ICollection<CardPartDTO>? AllParts { get; set; }
    [JsonPropertyName("card_faces")]
    public ICollection<CardFaceDTO>? CardFaces { get; set; }
    [JsonPropertyName("image_uris")]
    public CardImageUriDTO? ImageUriData { get; set; }
}

public record CardFaceDTO
{
    public string? Name { get; set; } 
    [JsonPropertyName("image_uris")] 
    public CardImageUriDTO? ImageUriData { get; set; }
}

public record CardImageUriDTO
{
    public string? Large { get; set; }
}

public record CardPartDTO
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Component { get; set; }
    public string? Uri { get; set; }
    [JsonPropertyName("type_line")]
    public string? TypeLine { get; set; }
}

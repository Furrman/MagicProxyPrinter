namespace Domain.Models.DTO.Moxfield;

public record DeckDTO(string? Name = null, MainDTO? Main = null, Dictionary<string, BoardDetailsDTO>? Boards = null);

public record MainDTO(string? Id = null, string? Scryfall_Id = null, string? UniqueCardId = null);

public record BoardDetailsDTO(Dictionary<string, CardDTO>? Cards = null);

public record CardDTO(int? Quantity = null, CardDetailsDTO? Card = null);

public record CardDetailsDTO(string? Id = null, string? Scryfall_Id = null, string? UniqueCardId = null, string? Name = null);
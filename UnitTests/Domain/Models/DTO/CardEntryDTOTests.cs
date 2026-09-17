using Domain.Models.DTO;

namespace UnitTests.Domain.Models.DTO;

public class CardEntryDTOTests
{
    [Fact]
    public void Equals_ReturnsFalse_WhenCardSidesDiffer()
    {
        var card1 = new CardEntryDTO
        {
            Name = "Lightning Bolt",
            Quantity = 1,
            CardSides = [new CardSideDTO { Name = "Lightning Bolt", ImageUrl = "https://example.com/a.png" }]
        };
        var card2 = new CardEntryDTO
        {
            Name = "Lightning Bolt",
            Quantity = 1,
            CardSides = [new CardSideDTO { Name = "Lightning Bolt", ImageUrl = "https://example.com/b.png" }]
        };

        Assert.False(card1.Equals(card2));
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenAllFieldsMatch()
    {
        var card1 = new CardEntryDTO
        {
            Name = "Lightning Bolt",
            Quantity = 2,
            CardSides = [new CardSideDTO { Name = "Lightning Bolt", ImageUrl = "https://example.com/a.png" }]
        };
        var card2 = new CardEntryDTO
        {
            Name = "Lightning Bolt",
            Quantity = 2,
            CardSides = [new CardSideDTO { Name = "Lightning Bolt", ImageUrl = "https://example.com/a.png" }]
        };

        Assert.True(card1.Equals(card2));
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenNameDiffers()
    {
        var card1 = new CardEntryDTO { Name = "Lightning Bolt", Quantity = 1, CardSides = [] };
        var card2 = new CardEntryDTO { Name = "Shock", Quantity = 1, CardSides = [] };

        Assert.False(card1.Equals(card2));
    }

    [Fact]
    public void HashSet_DeduplicatesEntries_OnlyWhenCardSidesAreEqual()
    {
        var duplicate = new CardEntryDTO
        {
            Name = "Lightning Bolt",
            Quantity = 1,
            CardSides = [new CardSideDTO { Name = "Lightning Bolt", ImageUrl = "https://example.com/a.png" }]
        };
        var sameAsDuplicate = new CardEntryDTO
        {
            Name = "Lightning Bolt",
            Quantity = 1,
            CardSides = [new CardSideDTO { Name = "Lightning Bolt", ImageUrl = "https://example.com/a.png" }]
        };
        var differentSides = new CardEntryDTO
        {
            Name = "Lightning Bolt",
            Quantity = 1,
            CardSides = [new CardSideDTO { Name = "Lightning Bolt", ImageUrl = "https://example.com/different.png" }]
        };

        var set = new HashSet<CardEntryDTO> { duplicate, sameAsDuplicate, differentSides };

        Assert.Equal(2, set.Count);
    }
}

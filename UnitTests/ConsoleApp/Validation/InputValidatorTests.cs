using Moq;
using Domain.Services;
using ConsoleApp.Validation;

namespace UnitTests.ConsoleApp.Validation;

public class InputValidatorTests
{
    private readonly Mock<ILanguageService> _languageServiceMock;

    public InputValidatorTests()
    {
        _languageServiceMock = new Mock<ILanguageService>();
        _languageServiceMock.Setup(s => s.IsValidLanguage(It.IsAny<string?>())).Returns(true);
    }

    [Fact]
    public void Validate_WithNoDeckSourceProvided_ReturnsMissingInputError()
    {
        var errors = InputValidator.Validate(null, null, null, 1, _languageServiceMock.Object);

        Assert.Single(errors);
        Assert.Contains("path to exported deck", errors[0]);
    }

    [Fact]
    public void Validate_WithDeckFilePathOnly_DoesNotReturnMissingInputError()
    {
        var errors = InputValidator.Validate("deck.txt", null, null, 1, _languageServiceMock.Object);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_WithDeckUrlOnly_DoesNotReturnMissingInputError()
    {
        var errors = InputValidator.Validate(null, "https://moxfield.com/decks/abc", null, 1, _languageServiceMock.Object);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_WithInvalidLanguageCode_ReturnsLanguageErrors()
    {
        _languageServiceMock.Setup(s => s.IsValidLanguage("xx")).Returns(false);
        _languageServiceMock.Setup(s => s.AvailableLanguages).Returns("en, es, fr");

        var errors = InputValidator.Validate("deck.txt", null, "xx", 1, _languageServiceMock.Object);

        Assert.Equal(2, errors.Count);
        Assert.Equal("You have to specify correct language code.", errors[0]);
        Assert.Equal("Language codes: en, es, fr", errors[1]);
    }

    [Fact]
    public void Validate_WithValidLanguageCode_DoesNotReturnLanguageError()
    {
        _languageServiceMock.Setup(s => s.IsValidLanguage("en")).Returns(true);

        var errors = InputValidator.Validate("deck.txt", null, "en", 1, _languageServiceMock.Object);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithTooFewTokenCopies_ReturnsError(int tokenCopies)
    {
        var errors = InputValidator.Validate("deck.txt", null, null, tokenCopies, _languageServiceMock.Object);

        Assert.Single(errors);
        Assert.Equal("Number of copies for each token has to be greater than 0.", errors[0]);
    }

    [Fact]
    public void Validate_WithTooManyTokenCopies_ReturnsError()
    {
        var errors = InputValidator.Validate("deck.txt", null, null, 101, _languageServiceMock.Object);

        Assert.Single(errors);
        Assert.Equal("Number of copies for each token has to be less than 100.", errors[0]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(null)]
    public void Validate_WithValidTokenCopies_ReturnsNoErrors(int? tokenCopies)
    {
        var errors = InputValidator.Validate("deck.txt", null, null, tokenCopies, _languageServiceMock.Object);

        Assert.Empty(errors);
    }
}

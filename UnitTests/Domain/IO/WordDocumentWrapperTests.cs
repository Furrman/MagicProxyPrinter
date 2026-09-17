using DocumentFormat.OpenXml.Wordprocessing;
using OfficeIMO.Word;
using Domain.IO;

namespace UnitTests.Domain.IO;

public class WordDocumentWrapperTests : IDisposable
{
    private readonly string _filePath;

    public WordDocumentWrapperTests()
    {
        _filePath = Path.Combine(Path.GetTempPath(), $"MagicProxyPrinterTests_{Guid.NewGuid()}.docx");
    }

    public void Dispose()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }

    [Fact]
    public void SetMargins_BeforeCreate_ThrowsInvalidOperationException()
    {
        var wrapper = new WordDocumentWrapper();

        Assert.Throws<InvalidOperationException>(() => wrapper.SetMargins(WordMargin.Normal));
    }

    [Fact]
    public void SetOrientation_BeforeCreate_ThrowsInvalidOperationException()
    {
        var wrapper = new WordDocumentWrapper();

        Assert.Throws<InvalidOperationException>(() => wrapper.SetOrientation(PageOrientationValues.Portrait));
    }

    [Fact]
    public void SetPageSize_BeforeCreate_ThrowsInvalidOperationException()
    {
        var wrapper = new WordDocumentWrapper();

        Assert.Throws<InvalidOperationException>(() => wrapper.SetPageSize(WordPageSize.A4));
    }

    [Fact]
    public void AddParagraph_BeforeCreate_ThrowsInvalidOperationException()
    {
        var wrapper = new WordDocumentWrapper();

        Assert.Throws<InvalidOperationException>(() => wrapper.AddParagraph());
    }

    [Fact]
    public void Create_ThenSave_WritesFileToDisk()
    {
        using var wrapper = new WordDocumentWrapper();

        wrapper.Create(_filePath);
        wrapper.SetMargins(WordMargin.Normal);
        wrapper.SetOrientation(PageOrientationValues.Portrait);
        wrapper.SetPageSize(WordPageSize.A4);
        wrapper.AddParagraph();
        wrapper.Save();

        Assert.True(File.Exists(_filePath));
    }

    [Fact]
    public void Dispose_AfterCreate_DoesNotThrow()
    {
        var wrapper = new WordDocumentWrapper();
        wrapper.Create(_filePath);

        var exception = Record.Exception(wrapper.Dispose);

        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_WithoutCreate_DoesNotThrow()
    {
        var wrapper = new WordDocumentWrapper();

        var exception = Record.Exception(wrapper.Dispose);

        Assert.Null(exception);
    }
}

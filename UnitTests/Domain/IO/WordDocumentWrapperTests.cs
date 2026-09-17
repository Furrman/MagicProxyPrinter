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

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var wrapper = new WordDocumentWrapper();
        wrapper.Create(_filePath);
        wrapper.Dispose();

        var exception = Record.Exception(wrapper.Dispose);

        Assert.Null(exception);
    }

    [Fact]
    public void AddImage_SavesImage_EvenThoughSourceStreamIsDisposedImmediately()
    {
        // 1x1 transparent PNG - the smallest content OfficeIMO will accept as a real image.
        var imageBytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");

        using (var wrapper = new WordDocumentWrapper())
        {
            wrapper.Create(_filePath);
            var paragraph = wrapper.AddParagraph();

            // AddImage disposes its internal MemoryStream before returning - confirm OfficeIMO has
            // already consumed the bytes by then, not deferred them until Save().
            wrapper.AddImage(paragraph, imageBytes, "test-image", width: 10, height: 10);
            wrapper.Save();
        }

        using var reloaded = WordDocument.Load(_filePath);
        Assert.NotEmpty(reloaded.Images);
    }
}

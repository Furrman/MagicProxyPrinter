using Microsoft.Extensions.Logging;
using Moq;
using Domain.IO;

namespace UnitTests.Domain.IO;

public class FileManagerTests : IDisposable
{
    private readonly FileManager _fileManager;
    private readonly string _tempRoot;

    public FileManagerTests()
    {
        _fileManager = new FileManager(Mock.Of<ILogger<FileManager>>());
        _tempRoot = Path.Combine(Path.GetTempPath(), $"MagicProxyPrinterTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    [Fact]
    public void CreateOutputFolder_WithExistingPath_ReturnsFullPath()
    {
        var result = _fileManager.CreateOutputFolder(_tempRoot);

        Assert.Equal(Path.GetFullPath(_tempRoot), result);
    }

    [Fact]
    public void CreateOutputFolder_WithNonExistentPath_CreatesDirectoryAndReturnsFullPath()
    {
        var newFolder = Path.Combine(_tempRoot, "new-folder");

        var result = _fileManager.CreateOutputFolder(newFolder);

        Assert.Equal(Path.GetFullPath(newFolder), result);
        Assert.True(Directory.Exists(newFolder));
    }

    [Fact]
    public void DirectoryExists_ForExistingDirectory_ReturnsTrue()
    {
        Assert.True(_fileManager.DirectoryExists(_tempRoot));
    }

    [Fact]
    public void DirectoryExists_ForMissingDirectory_ReturnsFalse()
    {
        Assert.False(_fileManager.DirectoryExists(Path.Combine(_tempRoot, "missing")));
    }

    [Fact]
    public void FileExists_ForExistingFile_ReturnsTrue()
    {
        var filePath = Path.Combine(_tempRoot, "file.txt");
        File.WriteAllText(filePath, "content");

        Assert.True(_fileManager.FileExists(filePath));
    }

    [Fact]
    public void FileExists_ForMissingFile_ReturnsFalse()
    {
        Assert.False(_fileManager.FileExists(Path.Combine(_tempRoot, "missing.txt")));
    }

    [Fact]
    public void GetFilename_ReturnsFileNameWithoutExtension()
    {
        var result = _fileManager.GetFilename("/some/path/deck-list.txt");

        Assert.Equal("deck-list", result);
    }

    [Fact]
    public void GetLinesFromTextFile_ReturnsNonBlankLines()
    {
        var filePath = Path.Combine(_tempRoot, "deck.txt");
        File.WriteAllLines(filePath, ["1 Card A", "", "   ", "2 Card B"]);

        var result = _fileManager.GetLinesFromTextFile(filePath);

        Assert.NotNull(result);
        Assert.Equal(["1 Card A", "2 Card B"], result);
    }

    [Fact]
    public void GetLinesFromTextFile_ForMissingFile_ReturnsNull()
    {
        var result = _fileManager.GetLinesFromTextFile(Path.Combine(_tempRoot, "missing.txt"));

        Assert.Null(result);
    }

    [Fact]
    public void ReturnCorrectWordFilePath_WithExistingDirectory_ReturnsCombinedPath()
    {
        var result = _fileManager.ReturnCorrectWordFilePath(_tempRoot, "MyDeck");

        Assert.Equal(Path.Combine(_tempRoot, "MyDeck.docx"), result);
    }

    [Fact]
    public void ReturnCorrectWordFilePath_WithNonExistentDirectory_FallsBackToCurrentDirectory()
    {
        var result = _fileManager.ReturnCorrectWordFilePath(Path.Combine(_tempRoot, "missing"), "MyDeck");

        Assert.Equal(Path.Combine(Directory.GetCurrentDirectory(), "MyDeck.docx"), result);
    }

    [Fact]
    public void ReturnCorrectWordFilePath_WithEmptyDeckName_UsesDefaultName()
    {
        var result = _fileManager.ReturnCorrectWordFilePath(_tempRoot, string.Empty);

        Assert.Equal(Path.Combine(_tempRoot, "MagicProxyPrinter.docx"), result);
    }
}

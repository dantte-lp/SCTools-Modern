// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using NSubstitute;
using SCTools.Core.Models;
using SCTools.Core.Services;
using SCTools.Core.Services.Interfaces;
using Xunit;

namespace SCTools.Tests.Services;

public sealed class FileIndexServiceTests
{
    private readonly IFileSystem _fs = Substitute.For<IFileSystem>();
    private readonly FileIndexService _sut;

    public FileIndexServiceTests()
    {
        _sut = new FileIndexService(_fs);
    }

    // --- ComputeHashAsync ---
    [Fact]
    public async Task ComputeHashAsync_ReturnsHexEncodedSha256()
    {
        var content = "Hello, World!"u8.ToArray();
        _fs.OpenRead("/tmp/test.txt").Returns(new MemoryStream(content));

        var hash = await _sut.ComputeHashAsync("/tmp/test.txt", TestContext.Current.CancellationToken);

        // SHA-256 of "Hello, World!" is known
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().HaveLength(64); // SHA-256 produces 32 bytes = 64 hex chars
        hash.Should().MatchRegex("^[0-9A-F]{64}$");
    }

    [Fact]
    public async Task ComputeHashAsync_EmptyFile_ReturnsEmptyFileHash()
    {
        _fs.OpenRead("/tmp/empty.txt").Returns(new MemoryStream([]));

        var hash = await _sut.ComputeHashAsync("/tmp/empty.txt", TestContext.Current.CancellationToken);

        // SHA-256 of empty input is a known constant
        hash.Should().Be("E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855");
    }

    [Fact]
    public async Task ComputeHashAsync_SameContentProducesSameHash()
    {
        var content = "test data"u8.ToArray();
        _fs.OpenRead("/tmp/file1.txt").Returns(new MemoryStream(content));
        _fs.OpenRead("/tmp/file2.txt").Returns(new MemoryStream(content));

        var hash1 = await _sut.ComputeHashAsync("/tmp/file1.txt", TestContext.Current.CancellationToken);
        var hash2 = await _sut.ComputeHashAsync("/tmp/file2.txt", TestContext.Current.CancellationToken);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public async Task ComputeHashAsync_WithEmptyPath_ThrowsArgumentException()
    {
        var act = () => _sut.ComputeHashAsync(string.Empty, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // --- GetChangedFiles ---
    [Fact]
    public void GetChangedFiles_WhenAllMatch_ReturnsEmpty()
    {
        var entry = new FileHashEntry { RelativePath = "global.ini", Hash = "ABC123", Size = 100 };
        var local = new Dictionary<string, FileHashEntry>(StringComparer.OrdinalIgnoreCase) { ["global.ini"] = entry };
        var remote = new Dictionary<string, FileHashEntry>(StringComparer.OrdinalIgnoreCase) { ["global.ini"] = entry };

        var result = _sut.GetChangedFiles(local, remote);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetChangedFiles_WhenHashDiffers_ReturnsChanged()
    {
        var localEntry = new FileHashEntry { RelativePath = "global.ini", Hash = "AAA", Size = 100 };
        var remoteEntry = new FileHashEntry { RelativePath = "global.ini", Hash = "BBB", Size = 100 };
        var local = new Dictionary<string, FileHashEntry>(StringComparer.OrdinalIgnoreCase) { ["global.ini"] = localEntry };
        var remote = new Dictionary<string, FileHashEntry>(StringComparer.OrdinalIgnoreCase) { ["global.ini"] = remoteEntry };

        var result = _sut.GetChangedFiles(local, remote);

        result.Should().ContainSingle().Which.Should().Be("global.ini");
    }

    [Fact]
    public void GetChangedFiles_WhenSizeDiffers_ReturnsChanged()
    {
        var localEntry = new FileHashEntry { RelativePath = "global.ini", Hash = "AAA", Size = 100 };
        var remoteEntry = new FileHashEntry { RelativePath = "global.ini", Hash = "AAA", Size = 200 };
        var local = new Dictionary<string, FileHashEntry>(StringComparer.OrdinalIgnoreCase) { ["global.ini"] = localEntry };
        var remote = new Dictionary<string, FileHashEntry>(StringComparer.OrdinalIgnoreCase) { ["global.ini"] = remoteEntry };

        var result = _sut.GetChangedFiles(local, remote);

        result.Should().ContainSingle().Which.Should().Be("global.ini");
    }

    [Fact]
    public void GetChangedFiles_WhenNewFileInRemote_ReturnsNew()
    {
        var remoteEntry = new FileHashEntry { RelativePath = "new.ini", Hash = "ABC", Size = 50 };
        var local = new Dictionary<string, FileHashEntry>(StringComparer.OrdinalIgnoreCase);
        var remote = new Dictionary<string, FileHashEntry>(StringComparer.OrdinalIgnoreCase) { ["new.ini"] = remoteEntry };

        var result = _sut.GetChangedFiles(local, remote);

        result.Should().ContainSingle().Which.Should().Be("new.ini");
    }

    [Fact]
    public void GetChangedFiles_HashComparison_IsCaseInsensitive()
    {
        var localEntry = new FileHashEntry { RelativePath = "global.ini", Hash = "abc123", Size = 100 };
        var remoteEntry = new FileHashEntry { RelativePath = "global.ini", Hash = "ABC123", Size = 100 };
        var local = new Dictionary<string, FileHashEntry>(StringComparer.OrdinalIgnoreCase) { ["global.ini"] = localEntry };
        var remote = new Dictionary<string, FileHashEntry>(StringComparer.OrdinalIgnoreCase) { ["global.ini"] = remoteEntry };

        var result = _sut.GetChangedFiles(local, remote);

        result.Should().BeEmpty();
    }

    // --- SaveIndex / LoadIndex ---
    [Fact]
    public void SaveIndex_WritesJsonToFile()
    {
        var entry = new FileHashEntry { RelativePath = "global.ini", Hash = "ABC123", Size = 100 };
        var index = new Dictionary<string, FileHashEntry> { ["global.ini"] = entry };

        _sut.SaveIndex(index, "/tmp/index.json");

        _fs.Received(1).WriteAllText("/tmp/index.json", Arg.Is<string>(s => s.Contains("ABC123")));
    }

    [Fact]
    public void LoadIndex_WhenFileDoesNotExist_ReturnsEmptyDictionary()
    {
        _fs.FileExists("/tmp/index.json").Returns(false);

        var result = _sut.LoadIndex("/tmp/index.json");

        result.Should().BeEmpty();
    }

    [Fact]
    public void LoadIndex_WhenFileExists_DeserializesIndex()
    {
        var json = """
            {
              "global.ini": {
                "relativePath": "global.ini",
                "hash": "ABC123",
                "size": 100
              }
            }
            """;
        _fs.FileExists("/tmp/index.json").Returns(true);
        _fs.ReadAllText("/tmp/index.json").Returns(json);

        var result = _sut.LoadIndex("/tmp/index.json");

        result.Should().ContainKey("global.ini");
        result["global.ini"].Hash.Should().Be("ABC123");
        result["global.ini"].Size.Should().Be(100);
    }

    // --- BuildIndexAsync ---
    [Fact]
    public async Task BuildIndexAsync_WhenDirectoryMissing_ReturnsEmpty()
    {
        _fs.DirectoryExists("/missing").Returns(false);

        var result = await _sut.BuildIndexAsync("/missing", TestContext.Current.CancellationToken);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task BuildIndexAsync_WithEmptyPath_ThrowsArgumentException()
    {
        var act = () => _sut.BuildIndexAsync("  ", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}

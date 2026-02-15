// Licensed to the SCTools project under the MIT license.

using System.Net.Http.Headers;
using FluentAssertions;
using SCTools.Core.Helpers;
using Xunit;

namespace SCTools.Tests.Helpers;

public sealed class ContentValidatorTests
{
    // --- GetSafeFileName ---
    [Fact]
    public void GetSafeFileName_WithValidDisposition_ReturnsFileName()
    {
        var cd = new ContentDispositionHeaderValue("attachment") { FileName = "global.ini" };

        var result = ContentValidator.GetSafeFileName(cd);

        result.Should().Be("global.ini");
    }

    [Fact]
    public void GetSafeFileName_WithQuotedFileName_StripsQuotes()
    {
        var cd = new ContentDispositionHeaderValue("attachment") { FileName = "\"global.ini\"" };

        var result = ContentValidator.GetSafeFileName(cd);

        result.Should().Be("global.ini");
    }

    [Fact]
    public void GetSafeFileName_WithFileNameStar_PrefersIt()
    {
        var cd = new ContentDispositionHeaderValue("attachment")
        {
            FileName = "fallback.ini",
            FileNameStar = "global.ini",
        };

        var result = ContentValidator.GetSafeFileName(cd);

        result.Should().Be("global.ini");
    }

    [Fact]
    public void GetSafeFileName_WithNull_ReturnsNull()
    {
        var result = ContentValidator.GetSafeFileName(null);

        result.Should().BeNull();
    }

    [Fact]
    public void GetSafeFileName_WithEmptyFileName_ReturnsNull()
    {
        var cd = new ContentDispositionHeaderValue("attachment") { FileName = string.Empty };

        var result = ContentValidator.GetSafeFileName(cd);

        result.Should().BeNull();
    }

    [Fact]
    public void GetSafeFileName_WithDisallowedExtension_ReturnsNull()
    {
        var cd = new ContentDispositionHeaderValue("attachment") { FileName = "malware.exe" };

        var result = ContentValidator.GetSafeFileName(cd);

        result.Should().BeNull();
    }

    // --- GetSafeFileNameFromUrl ---
    [Fact]
    public void GetSafeFileNameFromUrl_WithValidUrl_ReturnsFileName()
    {
        var url = new Uri("https://github.com/repo/releases/download/v1.0/global.ini");

        var result = ContentValidator.GetSafeFileNameFromUrl(url);

        result.Should().Be("global.ini");
    }

    [Fact]
    public void GetSafeFileNameFromUrl_WithQueryString_StripsQuery()
    {
        var url = new Uri("https://example.com/files/data.zip?token=abc123");

        var result = ContentValidator.GetSafeFileNameFromUrl(url);

        result.Should().Be("data.zip");
    }

    [Fact]
    public void GetSafeFileNameFromUrl_WithNull_ReturnsNull()
    {
        var result = ContentValidator.GetSafeFileNameFromUrl(null);

        result.Should().BeNull();
    }

    [Fact]
    public void GetSafeFileNameFromUrl_WithDisallowedExtension_ReturnsNull()
    {
        var url = new Uri("https://example.com/files/script.sh");

        var result = ContentValidator.GetSafeFileNameFromUrl(url);

        result.Should().BeNull();
    }

    // --- IsValidFileName ---
    [Theory]
    [InlineData("global.ini", true)]
    [InlineData("data.zip", true)]
    [InlineData("archive.7z", true)]
    [InlineData("pack.tar", true)]
    [InlineData("file.gz", true)]
    [InlineData("script.exe", false)]
    [InlineData("../etc/passwd", false)]
    [InlineData("", false)]
    [InlineData("  ", false)]
    public void IsValidFileName_ReturnsExpected(string fileName, bool expected)
    {
        var result = ContentValidator.IsValidFileName(fileName);

        result.Should().Be(expected);
    }

    [Fact]
    public void IsValidFileName_WithPathTraversal_ReturnsFalse()
    {
        ContentValidator.IsValidFileName("../../evil.ini").Should().BeFalse();
    }

    [Fact]
    public void IsValidFileName_WithBackslash_ReturnsFalse()
    {
        ContentValidator.IsValidFileName("sub\\file.ini").Should().BeFalse();
    }
}

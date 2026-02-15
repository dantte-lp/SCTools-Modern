// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using SCTools.Core.Helpers;
using Xunit;

namespace SCTools.Tests.Helpers;

public sealed class PathValidatorTests
{
    private const string BasePath = "/game/StarCitizen/LIVE";

    [Fact]
    public void SafeCombine_WithValidRelativePath_ReturnsFullPath()
    {
        var result = PathValidator.SafeCombine(BasePath, "data/localization");

        result.Should().StartWith(Path.GetFullPath(BasePath));
        result.Should().EndWith(Path.Combine("data", "localization"));
    }

    [Fact]
    public void SafeCombine_WithSimpleFileName_ReturnsFullPath()
    {
        var result = PathValidator.SafeCombine(BasePath, "user.cfg");

        result.Should().Be(Path.GetFullPath(Path.Combine(BasePath, "user.cfg")));
    }

    [Fact]
    public void SafeCombine_WithTraversalAttack_ThrowsArgumentException()
    {
        var act = () => PathValidator.SafeCombine(BasePath, "../../etc/passwd");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("relativePath");
    }

    [Fact]
    public void SafeCombine_WithAbsolutePathEscape_ThrowsArgumentException()
    {
        var act = () => PathValidator.SafeCombine(BasePath, "../../../tmp/evil");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("relativePath");
    }

    [Fact]
    public void SafeCombine_WithNullBasePath_ThrowsArgumentException()
    {
        var act = () => PathValidator.SafeCombine(null!, "file.txt");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SafeCombine_WithEmptyRelativePath_ThrowsArgumentException()
    {
        var act = () => PathValidator.SafeCombine(BasePath, string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SafeCombine_WithWhitespaceBasePath_ThrowsArgumentException()
    {
        var act = () => PathValidator.SafeCombine("   ", "file.txt");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SafeCombine_WithNestedSubdirectory_ReturnsFullPath()
    {
        var result = PathValidator.SafeCombine(BasePath, "data/localization/russian/global.ini");

        result.Should().Contain("russian");
        result.Should().EndWith("global.ini");
    }

    [Fact]
    public void IsSafePath_WithValidPath_ReturnsTrue()
    {
        var result = PathValidator.IsSafePath(BasePath, "data/localization/russian");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsSafePath_WithTraversalAttack_ReturnsFalse()
    {
        var result = PathValidator.IsSafePath(BasePath, "../../etc/passwd");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsSafePath_WithNullBasePath_ReturnsFalse()
    {
        var result = PathValidator.IsSafePath(null!, "file.txt");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsSafePath_WithEmptyRelativePath_ReturnsFalse()
    {
        var result = PathValidator.IsSafePath(BasePath, string.Empty);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsSafePath_WithWhitespaceBasePath_ReturnsFalse()
    {
        var result = PathValidator.IsSafePath("  ", "file.txt");

        result.Should().BeFalse();
    }
}

// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using SCTools.Core.Helpers;
using Xunit;

namespace SCTools.Tests.Helpers;

public sealed class SemanticVersionComparerTests
{
    // --- Compare ---
    [Theory]
    [InlineData("1.0.0", "1.0.0", 0)]
    [InlineData("1.0.0", "2.0.0", -1)]
    [InlineData("2.0.0", "1.0.0", 1)]
    [InlineData("1.1.0", "1.0.0", 1)]
    [InlineData("1.0.1", "1.0.0", 1)]
    [InlineData("1.0.0", "1.0.1", -1)]
    public void Compare_ReturnsCorrectSign(string v1, string v2, int expectedSign)
    {
        var result = SemanticVersionComparer.Compare(v1, v2);

        Math.Sign(result).Should().Be(expectedSign);
    }

    [Theory]
    [InlineData("v1.0.0", "1.0.0", 0)]
    [InlineData("V2.0.0", "2.0.0", 0)]
    public void Compare_StripsVPrefix(string v1, string v2, int expectedSign)
    {
        var result = SemanticVersionComparer.Compare(v1, v2);

        Math.Sign(result).Should().Be(expectedSign);
    }

    [Fact]
    public void Compare_PreReleaseIsLessThanRelease()
    {
        var result = SemanticVersionComparer.Compare("1.0.0-beta", "1.0.0");

        result.Should().BeNegative();
    }

    [Fact]
    public void Compare_PreReleasesComparedAlphabetically()
    {
        var result = SemanticVersionComparer.Compare("1.0.0-alpha", "1.0.0-beta");

        result.Should().BeNegative();
    }

    [Fact]
    public void Compare_TwoIdenticalPreReleases_AreEqual()
    {
        var result = SemanticVersionComparer.Compare("1.0.0-beta", "1.0.0-beta");

        result.Should().Be(0);
    }

    [Fact]
    public void Compare_BuildMetadataIsIgnored()
    {
        var result = SemanticVersionComparer.Compare("1.0.0+build1", "1.0.0+build2");

        result.Should().Be(0);
    }

    [Theory]
    [InlineData(null, null, 0)]
    [InlineData(null, "1.0.0", -1)]
    [InlineData("1.0.0", null, 1)]
    public void Compare_HandlesNullVersions(string? v1, string? v2, int expectedSign)
    {
        var result = SemanticVersionComparer.Compare(v1, v2);

        Math.Sign(result).Should().Be(expectedSign);
    }

    [Theory]
    [InlineData("", "", 0)]
    [InlineData("  ", "  ", 0)]
    public void Compare_HandlesEmptyAndWhitespace(string v1, string v2, int expectedSign)
    {
        var result = SemanticVersionComparer.Compare(v1, v2);

        Math.Sign(result).Should().Be(expectedSign);
    }

    [Fact]
    public void Compare_TwoPartVersion()
    {
        var result = SemanticVersionComparer.Compare("1.2", "1.2.0");

        result.Should().Be(0);
    }

    [Fact]
    public void Compare_SinglePartVersion()
    {
        var result = SemanticVersionComparer.Compare("2", "1.9.9");

        result.Should().BePositive();
    }

    // --- IsNewer ---
    [Fact]
    public void IsNewer_WhenCandidateIsNewer_ReturnsTrue()
    {
        SemanticVersionComparer.IsNewer("1.0.0", "2.0.0").Should().BeTrue();
    }

    [Fact]
    public void IsNewer_WhenCandidateIsOlder_ReturnsFalse()
    {
        SemanticVersionComparer.IsNewer("2.0.0", "1.0.0").Should().BeFalse();
    }

    [Fact]
    public void IsNewer_WhenSameVersion_ReturnsFalse()
    {
        SemanticVersionComparer.IsNewer("1.0.0", "1.0.0").Should().BeFalse();
    }

    [Fact]
    public void IsNewer_WithVPrefix_ComparesCorrectly()
    {
        SemanticVersionComparer.IsNewer("v1.0.0", "v2.0.0").Should().BeTrue();
    }

    [Fact]
    public void IsNewer_NullCurrent_CandidateIsNewer()
    {
        SemanticVersionComparer.IsNewer(null, "1.0.0").Should().BeTrue();
    }

    [Fact]
    public void IsNewer_NullCandidate_ReturnsFalse()
    {
        SemanticVersionComparer.IsNewer("1.0.0", null).Should().BeFalse();
    }

    // --- Parse (internal) ---
    [Fact]
    public void Parse_FullVersion_ExtractsAllParts()
    {
        var parsed = SemanticVersionComparer.Parse("v1.2.3-beta+build42");

        parsed.Major.Should().Be(1);
        parsed.Minor.Should().Be(2);
        parsed.Patch.Should().Be(3);
        parsed.PreRelease.Should().Be("beta");
    }

    [Fact]
    public void Parse_SimpleVersion_DefaultsToZero()
    {
        var parsed = SemanticVersionComparer.Parse("1");

        parsed.Major.Should().Be(1);
        parsed.Minor.Should().Be(0);
        parsed.Patch.Should().Be(0);
        parsed.PreRelease.Should().BeEmpty();
    }

    [Fact]
    public void Parse_Null_ReturnsDefault()
    {
        var parsed = SemanticVersionComparer.Parse(null);

        parsed.Major.Should().Be(0);
        parsed.Minor.Should().Be(0);
        parsed.Patch.Should().Be(0);
    }
}

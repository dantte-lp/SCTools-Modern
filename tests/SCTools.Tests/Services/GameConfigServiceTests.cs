// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using NSubstitute;
using SCTools.Core.Models;
using SCTools.Core.Services;
using SCTools.Core.Services.Interfaces;
using Xunit;

namespace SCTools.Tests.Services;

public sealed class GameConfigServiceTests
{
    private const string GameModePath = "/game/StarCitizen/LIVE";

    private readonly IFileSystem _fs = Substitute.For<IFileSystem>();
    private readonly GameConfigService _sut;

    public GameConfigServiceTests()
    {
        _sut = new GameConfigService(_fs);
    }

    // --- FindValue (internal static) ---
    [Fact]
    public void FindValue_WithExistingKey_ReturnsValue()
    {
        var content = "g_language = russian\ng_languageAudio = english";

        var result = GameConfigService.FindValue(content, "g_language");

        result.Should().Be("russian");
    }

    [Fact]
    public void FindValue_WithMissingKey_ReturnsNull()
    {
        var content = "g_language = russian";

        var result = GameConfigService.FindValue(content, "g_nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public void FindValue_IsCaseInsensitive()
    {
        var content = "G_Language = russian";

        var result = GameConfigService.FindValue(content, "g_language");

        result.Should().Be("russian");
    }

    [Fact]
    public void FindValue_SkipsCommentedLines()
    {
        var content = "; g_language = commented\n// g_language = also_commented\n-- g_language = dash_comment\ng_language = actual";

        var result = GameConfigService.FindValue(content, "g_language");

        result.Should().Be("actual");
    }

    [Fact]
    public void FindValue_SkipsEmptyLines()
    {
        var content = "\n\ng_language = russian\n\n";

        var result = GameConfigService.FindValue(content, "g_language");

        result.Should().Be("russian");
    }

    [Fact]
    public void FindValue_TrimsWhitespace()
    {
        var content = "  g_language  =  russian  ";

        var result = GameConfigService.FindValue(content, "g_language");

        result.Should().Be("russian");
    }

    [Fact]
    public void FindValue_WithNoEqualsSign_SkipsLine()
    {
        var content = "no-equals-sign\ng_language = russian";

        var result = GameConfigService.FindValue(content, "g_language");

        result.Should().Be("russian");
    }

    [Fact]
    public void FindValue_WithEmptyContent_ReturnsNull()
    {
        var result = GameConfigService.FindValue(string.Empty, "g_language");

        result.Should().BeNull();
    }

    // --- SetValueInContent (internal static) ---
    [Fact]
    public void SetValueInContent_WhenKeyExists_UpdatesValue()
    {
        var content = "g_language = english\ng_languageAudio = english";

        var result = GameConfigService.SetValueInContent(content, "g_language", "russian");

        result.Should().Contain("g_language = russian");
        result.Should().Contain("g_languageAudio = english");
    }

    [Fact]
    public void SetValueInContent_WhenKeyDoesNotExist_AppendsLine()
    {
        var content = "g_language = russian";

        var result = GameConfigService.SetValueInContent(content, "g_languageAudio", "english");

        result.Should().Contain("g_language = russian");
        result.Should().EndWith("g_languageAudio = english");
    }

    [Fact]
    public void SetValueInContent_PreservesComments()
    {
        var content = "; This is a comment\ng_language = english\n// Another comment";

        var result = GameConfigService.SetValueInContent(content, "g_language", "russian");

        result.Should().Contain("; This is a comment");
        result.Should().Contain("// Another comment");
        result.Should().Contain("g_language = russian");
    }

    [Fact]
    public void SetValueInContent_PreservesBlankLines()
    {
        var content = "g_language = english\n\ng_languageAudio = english";

        var result = GameConfigService.SetValueInContent(content, "g_language", "russian");

        var lines = result.Split(Environment.NewLine);
        lines.Should().HaveCount(3);
        lines[1].Should().BeEmpty();
    }

    [Fact]
    public void SetValueInContent_WithEmptyContent_AddsNewLine()
    {
        var result = GameConfigService.SetValueInContent(string.Empty, "g_language", "russian");

        result.Should().Contain("g_language = russian");
    }

    [Fact]
    public void SetValueInContent_IsCaseInsensitiveForKey()
    {
        var content = "G_LANGUAGE = english";

        var result = GameConfigService.SetValueInContent(content, "g_language", "russian");

        result.Should().Contain("g_language = russian");
    }

    // --- RemoveValueFromContent (internal static) ---
    [Fact]
    public void RemoveValueFromContent_WhenKeyExists_RemovesLine()
    {
        var content = "g_language = russian\ng_languageAudio = english";

        var (removed, result) = GameConfigService.RemoveValueFromContent(content, "g_language");

        removed.Should().BeTrue();
        result.Should().NotContain("g_language = russian");
        result.Should().Contain("g_languageAudio = english");
    }

    [Fact]
    public void RemoveValueFromContent_WhenKeyDoesNotExist_ReturnsFalse()
    {
        var content = "g_language = russian";

        var (removed, result) = GameConfigService.RemoveValueFromContent(content, "g_nonexistent");

        removed.Should().BeFalse();
        result.Should().Contain("g_language = russian");
    }

    [Fact]
    public void RemoveValueFromContent_PreservesComments()
    {
        var content = "; comment\ng_language = russian\n// another";

        var (removed, result) = GameConfigService.RemoveValueFromContent(content, "g_language");

        removed.Should().BeTrue();
        result.Should().Contain("; comment");
        result.Should().Contain("// another");
    }

    [Fact]
    public void RemoveValueFromContent_IsCaseInsensitiveForKey()
    {
        var content = "G_LANGUAGE = english";

        var (removed, _) = GameConfigService.RemoveValueFromContent(content, "g_language");

        removed.Should().BeTrue();
    }

    [Fact]
    public void RemoveValueFromContent_WithEmptyContent_ReturnsFalse()
    {
        var (removed, _) = GameConfigService.RemoveValueFromContent(string.Empty, "g_language");

        removed.Should().BeFalse();
    }

    // --- Integration through public API ---
    [Fact]
    public void GetValue_WhenFileDoesNotExist_ReturnsNull()
    {
        _fs.FileExists(Arg.Any<string>()).Returns(false);

        var result = _sut.GetValue(GameModePath, "g_language");

        result.Should().BeNull();
    }

    [Fact]
    public void GetValue_WhenFileExists_ReturnsValue()
    {
        var cfgPath = GameConstants.GetUserConfigPath(GameModePath);
        _fs.FileExists(cfgPath).Returns(true);
        _fs.ReadAllText(cfgPath).Returns("g_language = russian");

        var result = _sut.GetValue(GameModePath, "g_language");

        result.Should().Be("russian");
    }

    [Fact]
    public void SetValue_WhenFileDoesNotExist_CreatesNew()
    {
        var cfgPath = GameConstants.GetUserConfigPath(GameModePath);
        _fs.FileExists(cfgPath).Returns(false);

        _sut.SetValue(GameModePath, "g_language", "russian");

        _fs.Received(1).WriteAllText(cfgPath, Arg.Is<string>(s => s.Contains("g_language = russian")));
    }

    [Fact]
    public void SetValue_WhenFileExists_UpdatesExisting()
    {
        var cfgPath = GameConstants.GetUserConfigPath(GameModePath);
        _fs.FileExists(cfgPath).Returns(true);
        _fs.ReadAllText(cfgPath).Returns("g_language = english");

        _sut.SetValue(GameModePath, "g_language", "russian");

        _fs.Received(1).WriteAllText(cfgPath, Arg.Is<string>(s => s.Contains("g_language = russian")));
    }

    [Fact]
    public void RemoveValue_WhenFileDoesNotExist_ReturnsFalse()
    {
        _fs.FileExists(Arg.Any<string>()).Returns(false);

        var result = _sut.RemoveValue(GameModePath, "g_language");

        result.Should().BeFalse();
    }

    [Fact]
    public void RemoveValue_WhenKeyExists_RemovesAndWritesBack()
    {
        var cfgPath = GameConstants.GetUserConfigPath(GameModePath);
        _fs.FileExists(cfgPath).Returns(true);
        _fs.ReadAllText(cfgPath).Returns("g_language = russian\ng_languageAudio = english");

        var result = _sut.RemoveValue(GameModePath, "g_language");

        result.Should().BeTrue();
        _fs.Received(1).WriteAllText(cfgPath, Arg.Is<string>(s => !s.Contains("g_language = russian")));
    }

    [Fact]
    public void RemoveValue_WhenKeyDoesNotExist_DoesNotWrite()
    {
        var cfgPath = GameConstants.GetUserConfigPath(GameModePath);
        _fs.FileExists(cfgPath).Returns(true);
        _fs.ReadAllText(cfgPath).Returns("g_language = russian");

        var result = _sut.RemoveValue(GameModePath, "g_nonexistent");

        result.Should().BeFalse();
        _fs.DidNotReceive().WriteAllText(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void GetValue_WithEmptyGameModePath_ThrowsArgumentException()
    {
        var act = () => _sut.GetValue(string.Empty, "key");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetValue_WithEmptyKey_ThrowsArgumentException()
    {
        var act = () => _sut.SetValue(GameModePath, " ", "value");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetValue_WithNullValue_ThrowsArgumentNullException()
    {
        var act = () => _sut.SetValue(GameModePath, "key", null!);

        act.Should().Throw<ArgumentNullException>();
    }
}

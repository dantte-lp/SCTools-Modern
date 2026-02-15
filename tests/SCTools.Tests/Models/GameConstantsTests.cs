// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using SCTools.Core.Models;
using Xunit;

namespace SCTools.Tests.Models;

/// <summary>
/// Tests for <see cref="GameConstants"/>.
/// </summary>
public sealed class GameConstantsTests
{
    [Theory]
    [InlineData(GameMode.Live, "LIVE")]
    [InlineData(GameMode.Ptu, "PTU")]
    [InlineData(GameMode.Eptu, "EPTU")]
    public void GetModeFolderName_ReturnsUppercaseName(GameMode mode, string expected)
    {
        GameConstants.GetModeFolderName(mode).Should().Be(expected);
    }

    [Fact]
    public void GetGameModePath_CombinesRootAndMode()
    {
        var root = Path.Combine("C:", "Games", "StarCitizen");

        var result = GameConstants.GetGameModePath(root, GameMode.Live);

        result.Should().Be(Path.Combine(root, "LIVE"));
    }

    [Fact]
    public void GetExePath_IncludesBin64AndExeName()
    {
        var modePath = Path.Combine("C:", "Games", "StarCitizen", "LIVE");

        var result = GameConstants.GetExePath(modePath);

        result.Should().Be(Path.Combine(modePath, "Bin64", "StarCitizen.exe"));
    }

    [Fact]
    public void GetDataPath_ReturnsDataSubfolder()
    {
        var modePath = Path.Combine("C:", "Games", "LIVE");

        GameConstants.GetDataPath(modePath)
            .Should().Be(Path.Combine(modePath, "data"));
    }

    [Fact]
    public void GetLocalizationPath_ReturnsNestedLocalizationFolder()
    {
        var modePath = Path.Combine("C:", "Games", "LIVE");

        GameConstants.GetLocalizationPath(modePath)
            .Should().Be(Path.Combine(modePath, "data", "localization"));
    }

    [Fact]
    public void GetUserConfigPath_ReturnsUserCfg()
    {
        var modePath = Path.Combine("C:", "Games", "LIVE");

        GameConstants.GetUserConfigPath(modePath)
            .Should().Be(Path.Combine(modePath, "user.cfg"));
    }

    [Fact]
    public void Constants_HaveExpectedValues()
    {
        GameConstants.GameFolderName.Should().Be("StarCitizen");
        GameConstants.BinFolderName.Should().Be("Bin64");
        GameConstants.GameExeName.Should().Be("StarCitizen.exe");
        GameConstants.UserConfigName.Should().Be("user.cfg");
        GameConstants.GlobalIniName.Should().Be("global.ini");
        GameConstants.LanguageKey.Should().Be("g_language");
        GameConstants.DefaultLanguage.Should().Be("english");
    }
}

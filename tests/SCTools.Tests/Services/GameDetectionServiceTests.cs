// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using NSubstitute;
using SCTools.Core.Models;
using SCTools.Core.Services;
using SCTools.Core.Services.Interfaces;
using Xunit;

namespace SCTools.Tests.Services;

/// <summary>
/// Tests for <see cref="GameDetectionService"/>.
/// </summary>
public sealed class GameDetectionServiceTests
{
    private static readonly string TestRoot =
        Path.Combine(Path.DirectorySeparatorChar + "game", "StarCitizen");

    private readonly IFileSystem _fs = Substitute.For<IFileSystem>();
    private readonly GameDetectionService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameDetectionServiceTests"/> class.
    /// </summary>
    public GameDetectionServiceTests()
    {
        _sut = new GameDetectionService(_fs);
    }

    [Fact]
    public void DetectInstallations_WhenNoModesExist_ReturnsEmpty()
    {
        var result = _sut.DetectInstallations(TestRoot);

        result.Should().BeEmpty();
    }

    [Fact]
    public void DetectInstallations_WhenLiveExists_ReturnsSingleInstallation()
    {
        SetupGameMode(TestRoot, GameMode.Live, "3.24.0");

        var result = _sut.DetectInstallations(TestRoot);

        result.Should().ContainSingle();
        result[0].Mode.Should().Be(GameMode.Live);
        result[0].ExeVersion.Should().Be("3.24.0");
    }

    [Fact]
    public void DetectInstallations_WhenMultipleModesExist_ReturnsAll()
    {
        SetupGameMode(TestRoot, GameMode.Live, "3.24.0");
        SetupGameMode(TestRoot, GameMode.Ptu, "3.24.1");

        var result = _sut.DetectInstallations(TestRoot);

        result.Should().HaveCount(2);
        result.Select(i => i.Mode).Should().Contain([GameMode.Live, GameMode.Ptu]);
    }

    [Fact]
    public void DetectInstallations_WhenDirectoryExistsButNoExe_SkipsMode()
    {
        var livePath = Path.Combine(TestRoot, "LIVE");
        _fs.DirectoryExists(livePath).Returns(true);
        var result = _sut.DetectInstallations(TestRoot);

        result.Should().BeEmpty();
    }

    [Fact]
    public void DetectInstallations_WhenVersionIsNull_IncludesInstallation()
    {
        var livePath = Path.Combine(TestRoot, "LIVE");
        var exePath = GameConstants.GetExePath(livePath);

        _fs.DirectoryExists(livePath).Returns(true);
        _fs.FileExists(exePath).Returns(true);
        _fs.GetFileVersion(exePath).Returns((string?)null);

        var result = _sut.DetectInstallations(TestRoot);

        result.Should().ContainSingle()
            .Which.ExeVersion.Should().BeNull();
    }

    [Fact]
    public void DetectInstallations_WithEmptyPath_ThrowsArgumentException()
    {
        var act = () => _sut.DetectInstallations(string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FindGameFolder_WhenDirectPathHasModes_ReturnsPath()
    {
        SetupDirectoryExists(TestRoot, GameMode.Live);

        var result = _sut.FindGameFolder(TestRoot);

        result.Should().Be(TestRoot);
    }

    [Fact]
    public void FindGameFolder_WhenInsideBin64_GoesUpTwoLevels()
    {
        var bin64 = Path.Combine(TestRoot, "LIVE", "Bin64");
        SetupDirectoryExists(TestRoot, GameMode.Live);

        var result = _sut.FindGameFolder(bin64);

        result.Should().Be(TestRoot);
    }

    [Fact]
    public void FindGameFolder_WhenInsideModeFolder_GoesUpOneLevel()
    {
        var modeFolder = Path.Combine(TestRoot, "LIVE");
        SetupDirectoryExists(TestRoot, GameMode.Live);

        var result = _sut.FindGameFolder(modeFolder);

        result.Should().Be(TestRoot);
    }

    [Fact]
    public void FindGameFolder_WhenStarCitizenSubfolderExists_ReturnsIt()
    {
        var parent = Path.DirectorySeparatorChar + "game";
        var scFolder = Path.Combine(parent, "StarCitizen");
        _fs.DirectoryExists(scFolder).Returns(true);
        SetupDirectoryExists(scFolder, GameMode.Live);

        var result = _sut.FindGameFolder(parent);

        result.Should().Be(scFolder);
    }

    [Fact]
    public void FindGameFolder_WhenNothingFound_ReturnsNull()
    {
        var path = Path.Combine(Path.DirectorySeparatorChar + "empty", "path");
        var result = _sut.FindGameFolder(path);

        result.Should().BeNull();
    }

    [Fact]
    public void FindGameFolder_WithEmptyPath_ThrowsArgumentException()
    {
        var act = () => _sut.FindGameFolder(" ");

        act.Should().Throw<ArgumentException>();
    }

    private void SetupGameMode(string root, GameMode mode, string version)
    {
        var modePath = GameConstants.GetGameModePath(root, mode);
        var exePath = GameConstants.GetExePath(modePath);

        _fs.DirectoryExists(modePath).Returns(true);
        _fs.FileExists(exePath).Returns(true);
        _fs.GetFileVersion(exePath).Returns(version);
    }

    private void SetupDirectoryExists(string root, GameMode mode)
    {
        var modePath = GameConstants.GetGameModePath(root, mode);
        _fs.DirectoryExists(modePath).Returns(true);
    }
}

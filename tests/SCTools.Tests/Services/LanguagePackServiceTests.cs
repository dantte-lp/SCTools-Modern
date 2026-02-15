// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using NSubstitute;
using SCTools.Core.Models;
using SCTools.Core.Services;
using SCTools.Core.Services.Interfaces;
using Xunit;

namespace SCTools.Tests.Services;

public sealed class LanguagePackServiceTests
{
    private const string GameModePath = "/game/StarCitizen/LIVE";

    private readonly IFileSystem _fs = Substitute.For<IFileSystem>();
    private readonly IGameConfigService _config = Substitute.For<IGameConfigService>();
    private readonly LanguagePackService _sut;

    public LanguagePackServiceTests()
    {
        _sut = new LanguagePackService(_fs, _config);
    }

    // --- GetInstalledLanguages ---
    [Fact]
    public void GetInstalledLanguages_WhenLocFolderMissing_ReturnsEmpty()
    {
        _fs.DirectoryExists(Arg.Any<string>()).Returns(false);

        var result = _sut.GetInstalledLanguages(GameModePath);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetInstalledLanguages_WhenLanguageFoldersExist_ReturnsLanguagePacks()
    {
        var locPath = GameConstants.GetLocalizationPath(GameModePath);
        _fs.DirectoryExists(locPath).Returns(true);
        _fs.GetDirectories(locPath).Returns([
            Path.Combine(locPath, "russian"),
            Path.Combine(locPath, "german"),
        ]);
        _fs.FileExists(Path.Combine(locPath, "russian", GameConstants.GlobalIniName)).Returns(true);
        _fs.FileExists(Path.Combine(locPath, "german", GameConstants.GlobalIniName)).Returns(false);

        var result = _sut.GetInstalledLanguages(GameModePath);

        result.Should().HaveCount(2);
        result[0].LanguageCode.Should().Be("russian");
        result[0].HasGlobalIni.Should().BeTrue();
        result[1].LanguageCode.Should().Be("german");
        result[1].HasGlobalIni.Should().BeFalse();
    }

    [Fact]
    public void GetInstalledLanguages_WithEmptyPath_ThrowsArgumentException()
    {
        var act = () => _sut.GetInstalledLanguages(string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    // --- GetCurrentLanguage ---
    [Fact]
    public void GetCurrentLanguage_DelegatesToConfigService()
    {
        _config.GetValue(GameModePath, GameConstants.LanguageKey).Returns("russian");

        var result = _sut.GetCurrentLanguage(GameModePath);

        result.Should().Be("russian");
    }

    [Fact]
    public void GetCurrentLanguage_WhenNotSet_ReturnsNull()
    {
        _config.GetValue(GameModePath, GameConstants.LanguageKey).Returns((string?)null);

        var result = _sut.GetCurrentLanguage(GameModePath);

        result.Should().BeNull();
    }

    // --- SetCurrentLanguage ---
    [Fact]
    public void SetCurrentLanguage_SetsBothLanguageKeys()
    {
        _sut.SetCurrentLanguage(GameModePath, "russian");

        _config.Received(1).SetValue(GameModePath, GameConstants.LanguageKey, "russian");
        _config.Received(1).SetValue(GameModePath, GameConstants.LanguageAudioKey, GameConstants.DefaultLanguage);
    }

    [Fact]
    public void SetCurrentLanguage_WithEmptyLanguageCode_ThrowsArgumentException()
    {
        var act = () => _sut.SetCurrentLanguage(GameModePath, " ");

        act.Should().Throw<ArgumentException>();
    }

    // --- ResetLanguage ---
    [Fact]
    public void ResetLanguage_RemovesBothLanguageKeys()
    {
        _sut.ResetLanguage(GameModePath);

        _config.Received(1).RemoveValue(GameModePath, GameConstants.LanguageKey);
        _config.Received(1).RemoveValue(GameModePath, GameConstants.LanguageAudioKey);
    }

    // --- InstallFromFile ---
    [Fact]
    public void InstallFromFile_WhenGameNotFound_ReturnsGameNotFound()
    {
        _fs.DirectoryExists(GameModePath).Returns(false);

        var result = _sut.InstallFromFile(GameModePath, "russian", "/tmp/global.ini");

        result.Should().Be(InstallStatus.GameNotFound);
    }

    [Fact]
    public void InstallFromFile_WhenSourceFileMissing_ReturnsPackageError()
    {
        _fs.DirectoryExists(GameModePath).Returns(true);
        _fs.FileExists("/tmp/global.ini").Returns(false);

        var result = _sut.InstallFromFile(GameModePath, "russian", "/tmp/global.ini");

        result.Should().Be(InstallStatus.PackageError);
    }

    [Fact]
    public void InstallFromFile_WithTraversalLanguageCode_ReturnsPathError()
    {
        _fs.DirectoryExists(GameModePath).Returns(true);
        _fs.FileExists("/tmp/global.ini").Returns(true);

        var result = _sut.InstallFromFile(GameModePath, "../../../etc", "/tmp/global.ini");

        result.Should().Be(InstallStatus.PathError);
    }

    [Fact]
    public void InstallFromFile_WithValidInput_CopiesFileAndReturnsSuccess()
    {
        _fs.DirectoryExists(GameModePath).Returns(true);
        _fs.FileExists("/tmp/global.ini").Returns(true);

        var result = _sut.InstallFromFile(GameModePath, "russian", "/tmp/global.ini");

        result.Should().Be(InstallStatus.Success);

        var expectedLangFolder = Path.Combine(
            GameConstants.GetLocalizationPath(GameModePath), "russian");
        _fs.Received(1).CreateDirectory(expectedLangFolder);
        _fs.Received(1).CopyFile(
            "/tmp/global.ini",
            Path.Combine(expectedLangFolder, GameConstants.GlobalIniName),
            overwrite: true);
    }

    [Fact]
    public void InstallFromFile_WhenIOExceptionOccurs_ReturnsFileError()
    {
        _fs.DirectoryExists(GameModePath).Returns(true);
        _fs.FileExists("/tmp/global.ini").Returns(true);
        _fs.When(x => x.CopyFile(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()))
            .Do(_ => throw new IOException("Disk full"));

        var result = _sut.InstallFromFile(GameModePath, "russian", "/tmp/global.ini");

        result.Should().Be(InstallStatus.FileError);
    }

    [Fact]
    public void InstallFromFile_WhenUnauthorizedAccessException_ReturnsFileError()
    {
        _fs.DirectoryExists(GameModePath).Returns(true);
        _fs.FileExists("/tmp/global.ini").Returns(true);
        _fs.When(x => x.CopyFile(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()))
            .Do(_ => throw new UnauthorizedAccessException("Access denied"));

        var result = _sut.InstallFromFile(GameModePath, "russian", "/tmp/global.ini");

        result.Should().Be(InstallStatus.FileError);
    }

    [Fact]
    public void InstallFromFile_WithEmptyGameModePath_ThrowsArgumentException()
    {
        var act = () => _sut.InstallFromFile(" ", "russian", "/tmp/global.ini");

        act.Should().Throw<ArgumentException>();
    }

    // --- Uninstall ---
    [Fact]
    public void Uninstall_WhenEnglish_ReturnsFalse()
    {
        var result = _sut.Uninstall(GameModePath, "english");

        result.Should().BeFalse();
        _fs.DidNotReceive().DeleteDirectory(Arg.Any<string>());
    }

    [Fact]
    public void Uninstall_WhenFolderDoesNotExist_ReturnsFalse()
    {
        _fs.DirectoryExists(Arg.Any<string>()).Returns(false);

        var result = _sut.Uninstall(GameModePath, "russian");

        result.Should().BeFalse();
    }

    [Fact]
    public void Uninstall_WhenFolderExists_DeletesAndReturnsTrue()
    {
        var langFolder = Path.Combine(GameConstants.GetLocalizationPath(GameModePath), "russian");
        _fs.DirectoryExists(langFolder).Returns(true);
        _config.GetValue(GameModePath, GameConstants.LanguageKey).Returns("german");

        var result = _sut.Uninstall(GameModePath, "russian");

        result.Should().BeTrue();
        _fs.Received(1).DeleteDirectory(langFolder);
    }

    [Fact]
    public void Uninstall_WhenCurrentLanguageMatches_ResetsLanguage()
    {
        var langFolder = Path.Combine(GameConstants.GetLocalizationPath(GameModePath), "russian");
        _fs.DirectoryExists(langFolder).Returns(true);
        _config.GetValue(GameModePath, GameConstants.LanguageKey).Returns("russian");

        _sut.Uninstall(GameModePath, "russian");

        _config.Received(1).RemoveValue(GameModePath, GameConstants.LanguageKey);
        _config.Received(1).RemoveValue(GameModePath, GameConstants.LanguageAudioKey);
    }

    [Fact]
    public void Uninstall_WhenCurrentLanguageDiffers_DoesNotResetLanguage()
    {
        var langFolder = Path.Combine(GameConstants.GetLocalizationPath(GameModePath), "russian");
        _fs.DirectoryExists(langFolder).Returns(true);
        _config.GetValue(GameModePath, GameConstants.LanguageKey).Returns("german");

        _sut.Uninstall(GameModePath, "russian");

        _config.DidNotReceive().RemoveValue(GameModePath, GameConstants.LanguageKey);
    }

    // --- UninstallAll ---
    [Fact]
    public void UninstallAll_WhenNoLanguagesInstalled_ReturnsFalse()
    {
        var locPath = GameConstants.GetLocalizationPath(GameModePath);
        _fs.DirectoryExists(locPath).Returns(false);

        var result = _sut.UninstallAll(GameModePath);

        result.Should().BeFalse();
    }

    [Fact]
    public void UninstallAll_SkipsEnglish_DeletesOthers()
    {
        var locPath = GameConstants.GetLocalizationPath(GameModePath);
        var russianPath = Path.Combine(locPath, "russian");
        var englishPath = Path.Combine(locPath, "english");

        _fs.DirectoryExists(locPath).Returns(true);
        _fs.GetDirectories(locPath).Returns([russianPath, englishPath]);
        _fs.FileExists(Arg.Any<string>()).Returns(true);
        _fs.DirectoryExists(russianPath).Returns(true);
        _fs.DirectoryExists(englishPath).Returns(true);

        var result = _sut.UninstallAll(GameModePath);

        result.Should().BeTrue();
        _fs.Received(1).DeleteDirectory(russianPath);
        _fs.DidNotReceive().DeleteDirectory(englishPath);
    }

    [Fact]
    public void UninstallAll_WhenLanguagesRemoved_ResetsLanguage()
    {
        var locPath = GameConstants.GetLocalizationPath(GameModePath);
        var russianPath = Path.Combine(locPath, "russian");

        _fs.DirectoryExists(locPath).Returns(true);
        _fs.GetDirectories(locPath).Returns([russianPath]);
        _fs.FileExists(Arg.Any<string>()).Returns(true);
        _fs.DirectoryExists(russianPath).Returns(true);

        _sut.UninstallAll(GameModePath);

        _config.Received(1).RemoveValue(GameModePath, GameConstants.LanguageKey);
        _config.Received(1).RemoveValue(GameModePath, GameConstants.LanguageAudioKey);
    }

    [Fact]
    public void UninstallAll_WithEmptyPath_ThrowsArgumentException()
    {
        var act = () => _sut.UninstallAll("  ");

        act.Should().Throw<ArgumentException>();
    }
}

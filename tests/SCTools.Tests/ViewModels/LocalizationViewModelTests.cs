// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using NSubstitute;
using SCTools.Core.Models;
using SCTools.Core.Services.Interfaces;
using SCTools.Core.ViewModels;
using Xunit;

namespace SCTools.Tests.ViewModels;

public sealed class LocalizationViewModelTests
{
    private readonly ILanguagePackService _languagePack = Substitute.For<ILanguagePackService>();
    private readonly ILocalizationUpdater _updater = Substitute.For<ILocalizationUpdater>();
    private readonly LocalizationViewModel _sut;

    public LocalizationViewModelTests()
    {
        _sut = new LocalizationViewModel(_languagePack, _updater);
    }

    // --- GameModePath ---
    [Fact]
    public void GameModePath_WhenSet_RefreshesInstalledLanguages()
    {
        var packs = new List<LanguagePack> { CreatePack("russian") };
        _languagePack.GetInstalledLanguages("/game/LIVE").Returns(packs);
        _languagePack.GetCurrentLanguage("/game/LIVE").Returns("russian");

        _sut.GameModePath = "/game/LIVE";

        _sut.InstalledLanguages.Should().HaveCount(1);
        _sut.CurrentLanguage.Should().Be("russian");
    }

    [Fact]
    public void GameModePath_WhenNull_ClearsEverything()
    {
        var packs = new List<LanguagePack> { CreatePack("russian") };
        _languagePack.GetInstalledLanguages("/game/LIVE").Returns(packs);
        _sut.GameModePath = "/game/LIVE";

        _sut.GameModePath = null;

        _sut.InstalledLanguages.Should().BeEmpty();
        _sut.CurrentLanguage.Should().BeNull();
        _sut.SelectedLanguage.Should().BeNull();
    }

    // --- RefreshInstalledLanguages ---
    [Fact]
    public void Refresh_SelectsFirstLanguage()
    {
        var packs = new List<LanguagePack>
        {
            CreatePack("russian"),
            CreatePack("german"),
        };
        _languagePack.GetInstalledLanguages("/game/LIVE").Returns(packs);

        _sut.GameModePath = "/game/LIVE";

        _sut.SelectedLanguage!.LanguageCode.Should().Be("russian");
    }

    [Fact]
    public void Refresh_WhenNoLanguages_SelectedLanguageIsNull()
    {
        _languagePack.GetInstalledLanguages("/game/LIVE").Returns(new List<LanguagePack>());

        _sut.GameModePath = "/game/LIVE";

        _sut.SelectedLanguage.Should().BeNull();
    }

    // --- CheckForUpdateAsync ---
    [Fact]
    public async Task CheckForUpdate_WhenUpdateAvailable_SetsAvailableUpdate()
    {
        var release = CreateRelease("v1.1.0");
        _updater.CheckForUpdateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns(release);

        await _sut.CheckForUpdateCommand.ExecuteAsync(null);

        _sut.AvailableUpdate.Should().Be(release);
        _sut.StatusMessage.Should().Contain("v1.1.0");
        _sut.IsChecking.Should().BeFalse();
    }

    [Fact]
    public async Task CheckForUpdate_WhenUpToDate_ClearsAvailableUpdate()
    {
        _updater.CheckForUpdateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns((ReleaseInfo?)null);

        await _sut.CheckForUpdateCommand.ExecuteAsync(null);

        _sut.AvailableUpdate.Should().BeNull();
        _sut.StatusMessage.Should().Be("Localization is up to date.");
    }

    [Fact]
    public async Task CheckForUpdate_PassesRepositoryInfo()
    {
        _sut.RepositoryOwner = "myowner";
        _sut.RepositoryName = "myrepo";

        await _sut.CheckForUpdateCommand.ExecuteAsync(null);

        await _updater.Received(1).CheckForUpdateAsync(
            "myowner",
            "myrepo",
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckForUpdate_PassesCurrentVersion()
    {
        var packs = new List<LanguagePack> { CreatePack("russian", "v1.0.0") };
        _languagePack.GetInstalledLanguages("/game/LIVE").Returns(packs);
        _sut.GameModePath = "/game/LIVE";

        await _sut.CheckForUpdateCommand.ExecuteAsync(null);

        await _updater.Received(1).CheckForUpdateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            "v1.0.0",
            Arg.Any<CancellationToken>());
    }

    // --- InstallUpdateAsync ---
    [Fact]
    public async Task InstallUpdate_WhenNoUpdate_DoesNothing()
    {
        _sut.AvailableUpdate = null;

        await _sut.InstallUpdateCommand.ExecuteAsync(null);

        await _updater.DidNotReceive().UpdateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<ReleaseAssetInfo>(),
            Arg.Any<string?>(),
            Arg.Any<IProgress<DownloadProgressInfo>?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InstallUpdate_WhenSuccess_RefreshesLanguages()
    {
        _sut.GameModePath = "/game/LIVE";
        _sut.AvailableUpdate = CreateRelease("v1.1.0");
        _updater.UpdateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<ReleaseAssetInfo>(),
            Arg.Any<string?>(),
            Arg.Any<IProgress<DownloadProgressInfo>?>(),
            Arg.Any<CancellationToken>()).Returns(InstallStatus.Success);

        await _sut.InstallUpdateCommand.ExecuteAsync(null);

        _sut.StatusMessage.Should().Contain("successfully");
        _sut.IsInstalling.Should().BeFalse();
    }

    [Fact]
    public async Task InstallUpdate_WhenFailed_SetsErrorStatus()
    {
        _sut.GameModePath = "/game/LIVE";
        _sut.AvailableUpdate = CreateRelease("v1.1.0");
        _updater.UpdateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<ReleaseAssetInfo>(),
            Arg.Any<string?>(),
            Arg.Any<IProgress<DownloadProgressInfo>?>(),
            Arg.Any<CancellationToken>()).Returns(InstallStatus.FileError);

        await _sut.InstallUpdateCommand.ExecuteAsync(null);

        _sut.StatusMessage.Should().Contain("FileError");
    }

    // --- Uninstall ---
    [Fact]
    public void Uninstall_WhenNoSelection_DoesNothing()
    {
        _sut.SelectedLanguage = null;

        _sut.UninstallCommand.Execute(null);

        _languagePack.DidNotReceive().Uninstall(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void Uninstall_WhenSelected_RemovesLanguage()
    {
        _sut.GameModePath = "/game/LIVE";
        _sut.SelectedLanguage = CreatePack("russian");
        _languagePack.Uninstall("/game/LIVE", "russian").Returns(true);

        _sut.UninstallCommand.Execute(null);

        _languagePack.Received(1).Uninstall("/game/LIVE", "russian");
        _sut.StatusMessage.Should().Contain("uninstalled");
    }

    [Fact]
    public void Uninstall_WhenNotFound_SetsNotFoundStatus()
    {
        _sut.GameModePath = "/game/LIVE";
        _sut.SelectedLanguage = CreatePack("russian");
        _languagePack.Uninstall("/game/LIVE", "russian").Returns(false);

        _sut.UninstallCommand.Execute(null);

        _sut.StatusMessage.Should().Contain("not found");
    }

    // --- ResetLanguage ---
    [Fact]
    public void ResetLanguage_WhenNoPath_DoesNothing()
    {
        _sut.GameModePath = null;

        _sut.ResetLanguageCommand.Execute(null);

        _languagePack.DidNotReceive().ResetLanguage(Arg.Any<string>());
    }

    [Fact]
    public void ResetLanguage_ResetsToEnglish()
    {
        _sut.GameModePath = "/game/LIVE";

        _sut.ResetLanguageCommand.Execute(null);

        _languagePack.Received(1).ResetLanguage("/game/LIVE");
        _sut.CurrentLanguage.Should().BeNull();
        _sut.StatusMessage.Should().Be("Language reset to English.");
    }

    // --- RepositoryDefaults ---
    [Fact]
    public void DefaultRepository_IsH0useRus()
    {
        _sut.RepositoryOwner.Should().Be("h0useRus");
        _sut.RepositoryName.Should().Be("StarCitizen");
    }

    private static LanguagePack CreatePack(string code, string? version = null) => new()
    {
        LanguageCode = code,
        FolderPath = $"/game/data/localization/{code}",
        HasGlobalIni = true,
        Version = version,
    };

    private static ReleaseInfo CreateRelease(string tag) => new()
    {
        TagName = tag,
        Name = $"Release {tag}",
        Assets =
        [
            new ReleaseAssetInfo
            {
                FileName = "localization.zip",
                DownloadUrl = new Uri("https://example.com/localization.zip"),
            },
        ],
    };
}

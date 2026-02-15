// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using NSubstitute;
using SCTools.Core.Models;
using SCTools.Core.Services;
using SCTools.Core.Services.Interfaces;
using Xunit;

namespace SCTools.Tests.Services;

public sealed class LocalizationUpdaterTests
{
    private const string Owner = "h0useRus";
    private const string Repo = "StarCitizen";
    private const string GameModePath = "/game/StarCitizen/LIVE";

    private readonly IGitHubApiService _gitHubApi = Substitute.For<IGitHubApiService>();
    private readonly ILanguagePackService _languagePack = Substitute.For<ILanguagePackService>();
    private readonly IFileIndexService _fileIndex = Substitute.For<IFileIndexService>();
    private readonly IFileSystem _fs = Substitute.For<IFileSystem>();
    private readonly LocalizationUpdater _sut;

    public LocalizationUpdaterTests()
    {
        _sut = new LocalizationUpdater(_gitHubApi, _languagePack, _fileIndex, _fs);
    }

    // --- CheckForUpdateAsync ---
    [Fact]
    public async Task CheckForUpdateAsync_WhenNoRelease_ReturnsNull()
    {
        _gitHubApi.GetLatestReleaseAsync(Owner, Repo, false, Arg.Any<CancellationToken>())
            .Returns((ReleaseInfo?)null);

        var result = await _sut.CheckForUpdateAsync(Owner, Repo, null, TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenNoCurrentVersion_ReturnsLatest()
    {
        var release = CreateRelease("v1.0.0");
        _gitHubApi.GetLatestReleaseAsync(Owner, Repo, false, Arg.Any<CancellationToken>())
            .Returns(release);

        var result = await _sut.CheckForUpdateAsync(Owner, Repo, null, TestContext.Current.CancellationToken);

        result.Should().Be(release);
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenSameVersion_ReturnsNull()
    {
        var release = CreateRelease("v1.0.0");
        _gitHubApi.GetLatestReleaseAsync(Owner, Repo, false, Arg.Any<CancellationToken>())
            .Returns(release);

        var result = await _sut.CheckForUpdateAsync(Owner, Repo, "v1.0.0", TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenNewerVersion_ReturnsLatest()
    {
        var release = CreateRelease("v2.0.0");
        _gitHubApi.GetLatestReleaseAsync(Owner, Repo, false, Arg.Any<CancellationToken>())
            .Returns(release);

        var result = await _sut.CheckForUpdateAsync(Owner, Repo, "v1.0.0", TestContext.Current.CancellationToken);

        result.Should().Be(release);
    }

    // --- VerifyHashAsync ---
    [Fact]
    public async Task VerifyHashAsync_WhenNoExpectedHash_ReturnsTrue()
    {
        var result = await _sut.VerifyHashAsync("/tmp/file.ini", null, TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyHashAsync_WhenHashMatches_ReturnsTrue()
    {
        _fileIndex.ComputeHashAsync("/tmp/file.ini", Arg.Any<CancellationToken>())
            .Returns("ABC123");

        var result = await _sut.VerifyHashAsync("/tmp/file.ini", "abc123", TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyHashAsync_WhenHashDiffers_ReturnsFalse()
    {
        _fileIndex.ComputeHashAsync("/tmp/file.ini", Arg.Any<CancellationToken>())
            .Returns("ABC123");

        var result = await _sut.VerifyHashAsync("/tmp/file.ini", "DIFFERENT", TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }

    // --- UpdateAsync ---
    [Fact]
    public async Task UpdateAsync_WhenHashInvalid_ReturnsPackageError()
    {
        var asset = CreateAsset();
        SetupDownload(asset);
        _fileIndex.ComputeHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("WRONGHASH");

        var result = await _sut.UpdateAsync(
            GameModePath,
            "russian",
            asset,
            "EXPECTEDHASH",
            cancellationToken: TestContext.Current.CancellationToken);

        result.Should().Be(InstallStatus.PackageError);
    }

    [Fact]
    public async Task UpdateAsync_WhenNoHash_SkipsVerification()
    {
        var asset = CreateAsset();
        SetupDownload(asset);
        _languagePack.InstallFromFile(GameModePath, "russian", Arg.Any<string>())
            .Returns(InstallStatus.Success);

        var result = await _sut.UpdateAsync(
            GameModePath,
            "russian",
            asset,
            cancellationToken: TestContext.Current.CancellationToken);

        result.Should().Be(InstallStatus.Success);
    }

    [Fact]
    public async Task UpdateAsync_WhenInstallSucceeds_SetsLanguage()
    {
        var asset = CreateAsset();
        SetupDownload(asset);
        _languagePack.InstallFromFile(GameModePath, "russian", Arg.Any<string>())
            .Returns(InstallStatus.Success);

        await _sut.UpdateAsync(
            GameModePath,
            "russian",
            asset,
            cancellationToken: TestContext.Current.CancellationToken);

        _languagePack.Received(1).SetCurrentLanguage(GameModePath, "russian");
    }

    [Fact]
    public async Task UpdateAsync_WhenInstallFails_DoesNotSetLanguage()
    {
        var asset = CreateAsset();
        SetupDownload(asset);
        _languagePack.InstallFromFile(GameModePath, "russian", Arg.Any<string>())
            .Returns(InstallStatus.GameNotFound);

        await _sut.UpdateAsync(
            GameModePath,
            "russian",
            asset,
            cancellationToken: TestContext.Current.CancellationToken);

        _languagePack.DidNotReceive().SetCurrentLanguage(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task UpdateAsync_CleansUpTempFile()
    {
        var asset = CreateAsset();
        SetupDownload(asset);
        _languagePack.InstallFromFile(GameModePath, "russian", Arg.Any<string>())
            .Returns(InstallStatus.Success);
        _fs.FileExists(Arg.Any<string>()).Returns(true);

        await _sut.UpdateAsync(
            GameModePath,
            "russian",
            asset,
            cancellationToken: TestContext.Current.CancellationToken);

        _fs.Received().DeleteFile(Arg.Any<string>());
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyGameModePath_ThrowsArgumentException()
    {
        var asset = CreateAsset();

        var act = () => _sut.UpdateAsync(
            " ",
            "russian",
            asset,
            cancellationToken: TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    private static ReleaseInfo CreateRelease(string tagName) => new()
    {
        TagName = tagName,
        Name = $"Release {tagName}",
        Assets = [],
    };

    private static ReleaseAssetInfo CreateAsset() => new()
    {
        FileName = "global.ini",
        Size = 1024,
        DownloadUrl = new Uri("https://github.com/repo/releases/download/v1.0/global.ini"),
        ContentType = "application/octet-stream",
    };

    private void SetupDownload(ReleaseAssetInfo asset)
    {
        _gitHubApi.When(x => x.DownloadAssetAsync(
                asset.DownloadUrl, Arg.Any<Stream>(), Arg.Any<IProgress<long>>(), Arg.Any<CancellationToken>()))
            .Do(_ => { /* no-op: just "downloads" nothing */ });
    }
}

// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SCTools.Core.Models;
using SCTools.Core.Services;
using SCTools.Core.Services.Interfaces;
using Xunit;

namespace SCTools.Tests.Services;

public sealed class AutoUpdateServiceTests
{
    private readonly IUpdateManagerAdapter _adapter = Substitute.For<IUpdateManagerAdapter>();
    private readonly ILogger<AutoUpdateService> _logger = Substitute.For<ILogger<AutoUpdateService>>();
    private readonly AutoUpdateService _sut;

    public AutoUpdateServiceTests()
    {
        _sut = new AutoUpdateService(_adapter, _logger);
    }

    // --- IsInstalled / CurrentVersion ---
    [Fact]
    public void IsInstalled_DelegatesToAdapter()
    {
        _adapter.IsInstalled.Returns(true);

        _sut.IsInstalled.Should().BeTrue();
    }

    [Fact]
    public void CurrentVersion_DelegatesToAdapter()
    {
        _adapter.CurrentVersion.Returns("1.5.0");

        _sut.CurrentVersion.Should().Be("1.5.0");
    }

    // --- CheckForUpdateAsync ---
    [Fact]
    public async Task CheckForUpdateAsync_WhenNotInstalled_ReturnsNull()
    {
        _adapter.IsInstalled.Returns(false);

        var result = await _sut.CheckForUpdateAsync(TestContext.Current.CancellationToken);

        result.Should().BeNull();
        await _adapter.DidNotReceive().CheckForUpdateAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenUpToDate_ReturnsNull()
    {
        _adapter.IsInstalled.Returns(true);
        _adapter.CurrentVersion.Returns("1.0.0");
        _adapter.CheckForUpdateAsync(Arg.Any<CancellationToken>())
            .Returns((AppUpdateInfo?)null);

        var result = await _sut.CheckForUpdateAsync(TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenUpdateAvailable_ReturnsUpdateInfo()
    {
        var update = CreateUpdateInfo("2.0.0");
        _adapter.IsInstalled.Returns(true);
        _adapter.CurrentVersion.Returns("1.0.0");
        _adapter.CheckForUpdateAsync(Arg.Any<CancellationToken>())
            .Returns(update);

        var result = await _sut.CheckForUpdateAsync(TestContext.Current.CancellationToken);

        result.Should().Be(update);
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenExceptionOccurs_ReturnsNull()
    {
        _adapter.IsInstalled.Returns(true);
        _adapter.CheckForUpdateAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var result = await _sut.CheckForUpdateAsync(TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WhenCancelled_ThrowsOperationCanceled()
    {
        _adapter.IsInstalled.Returns(true);
        _adapter.CheckForUpdateAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        var act = () => _sut.CheckForUpdateAsync(TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // --- DownloadUpdateAsync ---
    [Fact]
    public async Task DownloadUpdateAsync_DelegatesToAdapter()
    {
        var update = CreateUpdateInfo("2.0.0");

        await _sut.DownloadUpdateAsync(update, cancellationToken: TestContext.Current.CancellationToken);

        await _adapter.Received(1).DownloadUpdateAsync(
            update,
            null,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DownloadUpdateAsync_PassesProgress()
    {
        var update = CreateUpdateInfo("2.0.0");
        var progress = Substitute.For<IProgress<int>>();

        await _sut.DownloadUpdateAsync(
            update,
            progress,
            TestContext.Current.CancellationToken);

        await _adapter.Received(1).DownloadUpdateAsync(
            update,
            progress,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DownloadUpdateAsync_WithNull_ThrowsArgumentNullException()
    {
        var act = () => _sut.DownloadUpdateAsync(null!, cancellationToken: TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // --- ApplyUpdateAndRestart / ApplyUpdateOnExit ---
    [Fact]
    public void ApplyUpdateAndRestart_DelegatesToAdapter()
    {
        _sut.ApplyUpdateAndRestart(["--updated"]);

        _adapter.Received(1).ApplyUpdateAndRestart(Arg.Is<string[]>(a => a[0] == "--updated"));
    }

    [Fact]
    public void ApplyUpdateOnExit_DelegatesToAdapter()
    {
        _sut.ApplyUpdateOnExit();

        _adapter.Received(1).ApplyUpdateOnExit();
    }

    private static AppUpdateInfo CreateUpdateInfo(string targetVersion) => new()
    {
        TargetVersion = targetVersion,
        CurrentVersion = "1.0.0",
    };
}

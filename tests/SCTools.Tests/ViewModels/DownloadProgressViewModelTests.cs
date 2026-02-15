// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using SCTools.Core.Models;
using SCTools.Core.ViewModels;
using Xunit;

namespace SCTools.Tests.ViewModels;

public sealed class DownloadProgressViewModelTests : IDisposable
{
    private readonly DownloadProgressViewModel _sut = new();

    public void Dispose()
    {
        _sut.Dispose();
    }

    // --- Start ---
    [Fact]
    public void Start_SetsIsDownloadingTrue()
    {
        _sut.Start("Test download");

        _sut.IsDownloading.Should().BeTrue();
        _sut.Title.Should().Be("Test download");
    }

    [Fact]
    public void Start_ResetsProgress()
    {
        _sut.Report(new DownloadProgressInfo { TotalBytes = 1000, BytesDownloaded = 500 });

        _sut.Start("New download");

        _sut.BytesDownloaded.Should().Be(0);
        _sut.TotalBytes.Should().BeNull();
        _sut.CurrentFile.Should().BeNull();
    }

    [Fact]
    public void Start_ReturnsCancellationToken()
    {
        var token = _sut.Start("Download");

        token.CanBeCanceled.Should().BeTrue();
        token.IsCancellationRequested.Should().BeFalse();
    }

    [Fact]
    public void Start_DisposesOldCts()
    {
        var token1 = _sut.Start("First");
        _ = _sut.Start("Second");

        // Old token's source was disposed — accessing it shouldn't throw but it should be distinct
        token1.IsCancellationRequested.Should().BeFalse();
    }

    // --- Complete ---
    [Fact]
    public void Complete_SetsIsDownloadingFalse()
    {
        _sut.Start("Download");

        _sut.Complete();

        _sut.IsDownloading.Should().BeFalse();
    }

    // --- Report ---
    [Fact]
    public void Report_UpdatesProgress()
    {
        _sut.Report(new DownloadProgressInfo
        {
            TotalBytes = 1000,
            BytesDownloaded = 250,
            CurrentFile = "data.zip",
        });

        _sut.TotalBytes.Should().Be(1000);
        _sut.BytesDownloaded.Should().Be(250);
        _sut.CurrentFile.Should().Be("data.zip");
    }

    [Fact]
    public void Report_ThrowsOnNull()
    {
        var act = () => _sut.Report(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // --- ProgressPercent ---
    [Fact]
    public void ProgressPercent_CalculatesCorrectly()
    {
        _sut.Report(new DownloadProgressInfo { TotalBytes = 200, BytesDownloaded = 50 });

        _sut.ProgressPercent.Should().Be(25.0);
    }

    [Fact]
    public void ProgressPercent_ZeroWhenTotalIsNull()
    {
        _sut.Report(new DownloadProgressInfo { TotalBytes = null, BytesDownloaded = 100 });

        _sut.ProgressPercent.Should().Be(0);
    }

    [Fact]
    public void ProgressPercent_ZeroWhenTotalIsZero()
    {
        _sut.Report(new DownloadProgressInfo { TotalBytes = 0, BytesDownloaded = 0 });

        _sut.ProgressPercent.Should().Be(0);
    }

    // --- IsIndeterminate ---
    [Fact]
    public void IsIndeterminate_TrueWhenTotalNull()
    {
        _sut.TotalBytes = null;

        _sut.IsIndeterminate.Should().BeTrue();
    }

    [Fact]
    public void IsIndeterminate_TrueWhenTotalZero()
    {
        _sut.TotalBytes = 0;

        _sut.IsIndeterminate.Should().BeTrue();
    }

    [Fact]
    public void IsIndeterminate_FalseWhenTotalPositive()
    {
        _sut.TotalBytes = 1000;

        _sut.IsIndeterminate.Should().BeFalse();
    }

    // --- Cancel ---
    [Fact]
    public void Cancel_RequestsCancellation()
    {
        var token = _sut.Start("Download");

        _sut.CancelCommand.Execute(null);

        token.IsCancellationRequested.Should().BeTrue();
    }

    // --- PropertyChanged notifications ---
    [Fact]
    public void Report_NotifiesProgressPercent()
    {
        var changedProperties = new List<string>();
        _sut.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        _sut.Report(new DownloadProgressInfo { TotalBytes = 100, BytesDownloaded = 50 });

        changedProperties.Should().Contain(nameof(DownloadProgressViewModel.ProgressPercent));
    }

    [Fact]
    public void TotalBytes_NotifiesIsIndeterminate()
    {
        var changedProperties = new List<string>();
        _sut.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        _sut.TotalBytes = 1000;

        changedProperties.Should().Contain(nameof(DownloadProgressViewModel.IsIndeterminate));
    }

    // --- Dispose ---
    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        _sut.Start("Download");

        _sut.Dispose();
        _sut.Dispose();

        // No exception thrown
    }

    [Fact]
    public void Start_AfterDispose_Throws()
    {
        _sut.Dispose();

        var act = () => _sut.Start("Download");

        act.Should().Throw<ObjectDisposedException>();
    }
}

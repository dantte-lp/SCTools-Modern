// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using SCTools.Core.Models;
using Xunit;

namespace SCTools.Tests.Models;

public sealed class DownloadProgressInfoTests
{
    [Fact]
    public void ProgressFraction_WhenTotalBytesKnown_ReturnsFraction()
    {
        var info = new DownloadProgressInfo { TotalBytes = 1000, BytesDownloaded = 500 };

        info.ProgressFraction.Should().Be(0.5);
    }

    [Fact]
    public void ProgressFraction_WhenTotalBytesNull_ReturnsNull()
    {
        var info = new DownloadProgressInfo { TotalBytes = null, BytesDownloaded = 500 };

        info.ProgressFraction.Should().BeNull();
    }

    [Fact]
    public void ProgressFraction_WhenTotalBytesZero_ReturnsNull()
    {
        var info = new DownloadProgressInfo { TotalBytes = 0, BytesDownloaded = 0 };

        info.ProgressFraction.Should().BeNull();
    }

    [Fact]
    public void ProgressFraction_WhenComplete_ReturnsOne()
    {
        var info = new DownloadProgressInfo { TotalBytes = 100, BytesDownloaded = 100 };

        info.ProgressFraction.Should().Be(1.0);
    }

    [Fact]
    public void CurrentFile_DefaultsToNull()
    {
        var info = new DownloadProgressInfo();

        info.CurrentFile.Should().BeNull();
    }
}

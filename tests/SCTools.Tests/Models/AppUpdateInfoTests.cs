// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using SCTools.Core.Models;
using Xunit;

namespace SCTools.Tests.Models;

public sealed class AppUpdateInfoTests
{
    [Fact]
    public void AppUpdateInfo_RequiredProperties_AreSet()
    {
        var info = new AppUpdateInfo
        {
            TargetVersion = "2.0.0",
            CurrentVersion = "1.0.0",
        };

        info.TargetVersion.Should().Be("2.0.0");
        info.CurrentVersion.Should().Be("1.0.0");
    }

    [Fact]
    public void AppUpdateInfo_OptionalProperties_DefaultToNull()
    {
        var info = new AppUpdateInfo
        {
            TargetVersion = "2.0.0",
            CurrentVersion = "1.0.0",
        };

        info.FileName.Should().BeNull();
        info.Size.Should().BeNull();
        info.ReleaseNotes.Should().BeNull();
        info.IsDowngrade.Should().BeFalse();
        info.HasDeltaUpdates.Should().BeFalse();
    }

    [Fact]
    public void AppUpdateInfo_WithAllProperties_SetsCorrectly()
    {
        var info = new AppUpdateInfo
        {
            TargetVersion = "2.0.0",
            CurrentVersion = "1.0.0",
            FileName = "SCTools-2.0.0-full.nupkg",
            Size = 10_000_000,
            ReleaseNotes = "# Release 2.0.0\n- New feature",
            IsDowngrade = false,
            HasDeltaUpdates = true,
        };

        info.FileName.Should().Be("SCTools-2.0.0-full.nupkg");
        info.Size.Should().Be(10_000_000);
        info.ReleaseNotes.Should().Contain("Release 2.0.0");
        info.HasDeltaUpdates.Should().BeTrue();
    }

    [Fact]
    public void UpdateStatus_HasExpectedValues()
    {
        Enum.GetValues<UpdateStatus>().Should().HaveCount(8);
        UpdateStatus.Idle.Should().Be(default);
    }
}

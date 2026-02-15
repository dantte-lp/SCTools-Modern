// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using SCTools.Core.Models;
using Xunit;

namespace SCTools.Tests.Models;

public sealed class ReleaseInfoTests
{
    [Fact]
    public void ReleaseInfo_RequiredProperties_AreSet()
    {
        var release = new ReleaseInfo
        {
            TagName = "v1.0.0",
            Name = "First Release",
        };

        release.TagName.Should().Be("v1.0.0");
        release.Name.Should().Be("First Release");
        release.Body.Should().BeNull();
        release.PublishedAt.Should().BeNull();
        release.IsPreRelease.Should().BeFalse();
        release.IsDraft.Should().BeFalse();
        release.HtmlUrl.Should().BeNull();
        release.Assets.Should().BeEmpty();
    }

    [Fact]
    public void ReleaseAssetInfo_RequiredProperties_AreSet()
    {
        var asset = new ReleaseAssetInfo
        {
            FileName = "app.zip",
            DownloadUrl = new Uri("https://example.com/app.zip"),
            Size = 1024,
            ContentType = "application/zip",
        };

        asset.FileName.Should().Be("app.zip");
        asset.DownloadUrl.Should().Be(new Uri("https://example.com/app.zip"));
        asset.Size.Should().Be(1024);
        asset.ContentType.Should().Be("application/zip");
    }

    [Fact]
    public void GitHubRateLimitInfo_Properties_AreSet()
    {
        var reset = DateTimeOffset.UtcNow.AddMinutes(30);
        var info = new GitHubRateLimitInfo
        {
            Limit = 5000,
            Remaining = 4999,
            ResetTime = reset,
        };

        info.Limit.Should().Be(5000);
        info.Remaining.Should().Be(4999);
        info.ResetTime.Should().Be(reset);
    }
}

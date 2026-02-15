// Licensed to the SCTools project under the MIT license.

using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Octokit;
using SCTools.Core.Exceptions;
using SCTools.Core.Services;
using Xunit;

namespace SCTools.Tests.Services;

public sealed class GitHubApiServiceTests : IDisposable
{
    private const string Owner = "h0useRus";
    private const string Repo = "StarCitizen";

    private readonly IGitHubClient _ghClient = Substitute.For<IGitHubClient>();
    private readonly IReleasesClient _releasesClient = Substitute.For<IReleasesClient>();
    private readonly IRateLimitClient _rateLimitClient = Substitute.For<IRateLimitClient>();
    private readonly FakeHandler _handler = new();
    private readonly HttpClient _httpClient;
    private readonly GitHubApiService _sut;

    public GitHubApiServiceTests()
    {
        _httpClient = new HttpClient(_handler);
        _ghClient.Repository.Release.Returns(_releasesClient);
        _ghClient.RateLimit.Returns(_rateLimitClient);
        _sut = new GitHubApiService(_ghClient, _httpClient);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _handler.Dispose();
    }

    [Fact]
    public async Task GetReleasesAsync_ReturnsMappedReleases()
    {
        var release = CreateRelease("v1.0.0", "Release 1.0", prerelease: false, draft: false);
        _releasesClient.GetAll(Owner, Repo).Returns([release]);

        var result = await _sut.GetReleasesAsync(Owner, Repo, cancellationToken: TestContext.Current.CancellationToken);

        result.Should().ContainSingle();
        result[0].TagName.Should().Be("v1.0.0");
        result[0].Name.Should().Be("Release 1.0");
        result[0].IsPreRelease.Should().BeFalse();
    }

    [Fact]
    public async Task GetReleasesAsync_ExcludesDrafts()
    {
        var release = CreateRelease("v1.0.0", "Draft", prerelease: false, draft: true);
        _releasesClient.GetAll(Owner, Repo).Returns([release]);

        var result = await _sut.GetReleasesAsync(Owner, Repo, cancellationToken: TestContext.Current.CancellationToken);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetReleasesAsync_ExcludesPreReleasesWhenNotRequested()
    {
        var stable = CreateRelease("v1.0.0", "Stable", prerelease: false, draft: false);
        var preRelease = CreateRelease("v2.0.0-beta", "Beta", prerelease: true, draft: false);
        _releasesClient.GetAll(Owner, Repo).Returns([stable, preRelease]);

        var result = await _sut.GetReleasesAsync(Owner, Repo, includePreReleases: false, cancellationToken: TestContext.Current.CancellationToken);

        result.Should().ContainSingle().Which.TagName.Should().Be("v1.0.0");
    }

    [Fact]
    public async Task GetReleasesAsync_IncludesPreReleasesWhenRequested()
    {
        var stable = CreateRelease("v1.0.0", "Stable", prerelease: false, draft: false);
        var preRelease = CreateRelease("v2.0.0-beta", "Beta", prerelease: true, draft: false);
        _releasesClient.GetAll(Owner, Repo).Returns([stable, preRelease]);

        var result = await _sut.GetReleasesAsync(Owner, Repo, includePreReleases: true, cancellationToken: TestContext.Current.CancellationToken);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetReleasesAsync_MapsAssets()
    {
        var release = CreateReleaseWithAsset(
            "v1.0.0",
            "Release",
            assetName: "file.zip",
            assetSize: 1024,
            assetUrl: "https://example.com/file.zip",
            assetContentType: "application/zip");
        _releasesClient.GetAll(Owner, Repo).Returns([release]);

        var result = await _sut.GetReleasesAsync(Owner, Repo, cancellationToken: TestContext.Current.CancellationToken);

        result[0].Assets.Should().ContainSingle();
        result[0].Assets[0].FileName.Should().Be("file.zip");
        result[0].Assets[0].Size.Should().Be(1024);
        result[0].Assets[0].DownloadUrl.Should().Be(new Uri("https://example.com/file.zip"));
        result[0].Assets[0].ContentType.Should().Be("application/zip");
    }

    [Fact]
    public async Task GetReleasesAsync_WhenRateLimited_ThrowsGitHubRateLimitException()
    {
        var response = CreateRateLimitResponse(0, 60, DateTimeOffset.UtcNow.AddMinutes(30));
        var rateLimitEx = new RateLimitExceededException(response);
        _releasesClient.GetAll(Owner, Repo).ThrowsAsync(rateLimitEx);

        var act = () => _sut.GetReleasesAsync(Owner, Repo, cancellationToken: TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<GitHubRateLimitException>()
            .Where(e => e.Remaining == 0 && e.Limit == 60);
    }

    [Fact]
    public async Task GetReleasesAsync_WithEmptyOwner_ThrowsArgumentException()
    {
        var act = () => _sut.GetReleasesAsync(string.Empty, Repo, cancellationToken: TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetLatestReleaseAsync_ReturnsLatest()
    {
        var release = CreateRelease("v2.0.0", "Latest", prerelease: false, draft: false);
        _releasesClient.GetLatest(Owner, Repo).Returns(release);

        var result = await _sut.GetLatestReleaseAsync(Owner, Repo, cancellationToken: TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.TagName.Should().Be("v2.0.0");
    }

    [Fact]
    public async Task GetLatestReleaseAsync_WhenNotFound_ReturnsNull()
    {
        _releasesClient.GetLatest(Owner, Repo)
            .ThrowsAsync(new NotFoundException("Not found", HttpStatusCode.NotFound));

        var result = await _sut.GetLatestReleaseAsync(Owner, Repo, cancellationToken: TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLatestReleaseAsync_WithPreReleases_FetchesAll()
    {
        var preRelease = CreateRelease("v3.0.0-beta", "Beta", prerelease: true, draft: false);
        var stable = CreateRelease("v2.0.0", "Stable", prerelease: false, draft: false);
        _releasesClient.GetAll(Owner, Repo).Returns([preRelease, stable]);

        var result = await _sut.GetLatestReleaseAsync(Owner, Repo, includePreReleases: true, cancellationToken: TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.TagName.Should().Be("v3.0.0-beta");
    }

    [Fact]
    public async Task GetRateLimitAsync_ReturnsMappedInfo()
    {
        var resetUnix = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();
        var rateLimits = new MiscellaneousRateLimit(
            new ResourceRateLimit(
                new RateLimit(5000, 4999, resetUnix),
                new RateLimit(30, 29, resetUnix),
                new RateLimit(0, 0, resetUnix)),
            new RateLimit(5000, 4999, resetUnix));
        _rateLimitClient.GetRateLimits().Returns(rateLimits);

        var result = await _sut.GetRateLimitAsync(TestContext.Current.CancellationToken);

        result.Limit.Should().Be(5000);
        result.Remaining.Should().Be(4999);
    }

    [Fact]
    public async Task DownloadAssetAsync_WritesToStream()
    {
        var content = "test file content"u8.ToArray();
        using var handler = new FakeHandler(content);
        using var httpClient = new HttpClient(handler);
        var sut = new GitHubApiService(_ghClient, httpClient);

        using var destination = new MemoryStream();
        await sut.DownloadAssetAsync(new Uri("https://example.com/file.zip"), destination, cancellationToken: TestContext.Current.CancellationToken);

        destination.ToArray().Should().Equal(content);
    }

    [Fact]
    public async Task DownloadAssetAsync_ReportsProgress()
    {
        var content = new byte[64 * 1024];
        Array.Fill(content, (byte)'A');
        using var handler = new FakeHandler(content);
        using var httpClient = new HttpClient(handler);
        var sut = new GitHubApiService(_ghClient, httpClient);
        var reported = new List<long>();
        var progress = new SynchronousProgress<long>(reported.Add);

        using var destination = new MemoryStream();
        await sut.DownloadAssetAsync(new Uri("https://example.com/file.zip"), destination, progress, TestContext.Current.CancellationToken);

        reported.Should().NotBeEmpty();
        reported[^1].Should().Be(content.Length);
    }

    [Fact]
    public async Task DownloadAssetAsync_WhenForbidden_ThrowsRateLimitException()
    {
        using var handler = new FakeHandler(statusCode: HttpStatusCode.Forbidden);
        using var httpClient = new HttpClient(handler);
        var sut = new GitHubApiService(_ghClient, httpClient);

        var act = () => sut.DownloadAssetAsync(
            new Uri("https://example.com/file.zip"),
            new MemoryStream(),
            cancellationToken: TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<GitHubRateLimitException>();
    }

    [Fact]
    public async Task DownloadAssetAsync_WithNullUrl_ThrowsArgumentNullException()
    {
        var act = () => _sut.DownloadAssetAsync(null!, new MemoryStream(), cancellationToken: TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private static Release CreateRelease(
        string tagName,
        string name,
        bool prerelease,
        bool draft)
    {
        return new Release(
            url: string.Empty,
            htmlUrl: $"https://github.com/{Owner}/{Repo}/releases/tag/{tagName}",
            assetsUrl: string.Empty,
            uploadUrl: string.Empty,
            id: 1,
            nodeId: string.Empty,
            tagName: tagName,
            targetCommitish: "main",
            name: name,
            body: $"Release {name}",
            draft: draft,
            prerelease: prerelease,
            createdAt: DateTimeOffset.UtcNow,
            publishedAt: DateTimeOffset.UtcNow,
            author: CreateAuthor(),
            tarballUrl: string.Empty,
            zipballUrl: string.Empty,
            assets: []);
    }

    private static Release CreateReleaseWithAsset(
        string tagName,
        string name,
        string assetName,
        int assetSize,
        string assetUrl,
        string assetContentType)
    {
        var asset = new ReleaseAsset(
            url: string.Empty,
            id: 1,
            nodeId: string.Empty,
            name: assetName,
            label: string.Empty,
            state: string.Empty,
            contentType: assetContentType,
            size: assetSize,
            downloadCount: 0,
            createdAt: DateTimeOffset.UtcNow,
            updatedAt: DateTimeOffset.UtcNow,
            browserDownloadUrl: assetUrl,
            uploader: CreateAuthor());

        return new Release(
            url: string.Empty,
            htmlUrl: $"https://github.com/{Owner}/{Repo}/releases/tag/{tagName}",
            assetsUrl: string.Empty,
            uploadUrl: string.Empty,
            id: 1,
            nodeId: string.Empty,
            tagName: tagName,
            targetCommitish: "main",
            name: name,
            body: $"Release {name}",
            draft: false,
            prerelease: false,
            createdAt: DateTimeOffset.UtcNow,
            publishedAt: DateTimeOffset.UtcNow,
            author: CreateAuthor(),
            tarballUrl: string.Empty,
            zipballUrl: string.Empty,
            assets: [asset]);
    }

    private static Author CreateAuthor()
    {
        return new Author(
            login: "test",
            id: 1,
            nodeId: string.Empty,
            avatarUrl: string.Empty,
            url: string.Empty,
            htmlUrl: string.Empty,
            followersUrl: string.Empty,
            followingUrl: string.Empty,
            gistsUrl: string.Empty,
            type: "User",
            starredUrl: string.Empty,
            subscriptionsUrl: string.Empty,
            organizationsUrl: string.Empty,
            reposUrl: string.Empty,
            eventsUrl: string.Empty,
            receivedEventsUrl: string.Empty,
            siteAdmin: false);
    }

    private static IResponse CreateRateLimitResponse(int remaining, int limit, DateTimeOffset reset)
    {
        var rateLimit = new RateLimit(limit, remaining, reset.ToUnixTimeSeconds());
        var apiInfo = new ApiInfo(
            new Dictionary<string, Uri>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            string.Empty,
            rateLimit,
            TimeSpan.Zero);

        var response = Substitute.For<IResponse>();
        response.StatusCode.Returns(HttpStatusCode.Forbidden);
        response.Body.Returns("rate limit exceeded");
        response.ContentType.Returns("application/json");
        response.ApiInfo.Returns(apiInfo);
        response.Headers.Returns(new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
        {
            ["X-RateLimit-Limit"] = limit.ToString(CultureInfo.InvariantCulture),
            ["X-RateLimit-Remaining"] = remaining.ToString(CultureInfo.InvariantCulture),
            ["X-RateLimit-Reset"] = reset.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
        }));
        return response;
    }

    private sealed class SynchronousProgress<T>(Action<T> handler) : IProgress<T>
    {
        public void Report(T value) => handler(value);
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly byte[] _content;
        private readonly HttpStatusCode _statusCode;

        public FakeHandler(byte[]? content = null, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _content = content ?? [];
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new ByteArrayContent(_content),
            };
            return Task.FromResult(response);
        }
    }
}

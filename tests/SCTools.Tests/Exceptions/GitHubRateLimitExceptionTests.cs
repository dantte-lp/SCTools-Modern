// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using SCTools.Core.Exceptions;
using Xunit;

namespace SCTools.Tests.Exceptions;

public sealed class GitHubRateLimitExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessage()
    {
        var ex = new GitHubRateLimitException();

        ex.Message.Should().Contain("rate limit");
    }

    [Fact]
    public void MessageConstructor_SetsMessage()
    {
        var ex = new GitHubRateLimitException("custom message");

        ex.Message.Should().Be("custom message");
    }

    [Fact]
    public void MessageAndInnerConstructor_SetsMessageAndInner()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new GitHubRateLimitException("outer", inner);

        ex.Message.Should().Be("outer");
        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void RateLimitConstructor_SetsAllProperties()
    {
        var reset = DateTimeOffset.UtcNow.AddMinutes(15);

        var ex = new GitHubRateLimitException(reset, limit: 60, remaining: 0);

        ex.ResetTime.Should().Be(reset);
        ex.Limit.Should().Be(60);
        ex.Remaining.Should().Be(0);
        ex.Message.Should().Contain("60");
        ex.Message.Should().Contain("Resets at");
    }

    [Fact]
    public void RateLimitException_IsHttpRequestException()
    {
        var ex = new GitHubRateLimitException();

        ex.Should().BeAssignableTo<HttpRequestException>();
    }
}

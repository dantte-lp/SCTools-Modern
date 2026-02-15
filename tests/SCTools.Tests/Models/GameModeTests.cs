// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using SCTools.Core.Models;
using Xunit;

namespace SCTools.Tests.Models;

/// <summary>
/// Tests for the <see cref="GameMode"/> enumeration.
/// </summary>
public sealed class GameModeTests
{
    [Fact]
    public void GameMode_ShouldHaveThreeValues()
    {
        Enum.GetValues<GameMode>().Should().HaveCount(3);
    }

    [Theory]
    [InlineData(GameMode.Live, 0)]
    [InlineData(GameMode.Ptu, 1)]
    [InlineData(GameMode.Eptu, 2)]
    public void GameMode_ShouldHaveExpectedIntegerValues(GameMode mode, int expected)
    {
        ((int)mode).Should().Be(expected);
    }

    [Theory]
    [InlineData(GameMode.Live)]
    [InlineData(GameMode.Ptu)]
    [InlineData(GameMode.Eptu)]
    public void GameMode_ShouldBeDefined(GameMode mode)
    {
        Enum.IsDefined(mode).Should().BeTrue();
    }
}

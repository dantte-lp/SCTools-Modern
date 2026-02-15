// Licensed to the SCTools project under the MIT license.

using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using SCTools.Core.Models;
using Xunit;

namespace SCTools.Tests.Models;

/// <summary>
/// Tests for <see cref="AppSettings"/> defaults and validation.
/// </summary>
public sealed class AppSettingsTests
{
    [Fact]
    public void Defaults_ShouldHaveExpectedValues()
    {
        var settings = new AppSettings();

        settings.GameFolder.Should().BeNull();
        settings.RunMinimized.Should().BeFalse();
        settings.AllowIncrementalDownload.Should().BeTrue();
        settings.RegularCheckForUpdates.Should().BeTrue();
        settings.UpdateCheckIntervalMinutes.Should().Be(60);
    }

    [Fact]
    public void SectionName_ShouldBeApp()
    {
        AppSettings.SectionName.Should().Be("App");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1441)]
    public void UpdateCheckIntervalMinutes_OutOfRange_FailsValidation(int invalidValue)
    {
        var settings = new AppSettings { UpdateCheckIntervalMinutes = invalidValue };
        var context = new ValidationContext(settings);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(settings, context, results, validateAllProperties: true)
            .Should().BeFalse();

        results.Should().ContainSingle()
            .Which.MemberNames.Should().Contain(nameof(AppSettings.UpdateCheckIntervalMinutes));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(60)]
    [InlineData(1440)]
    public void UpdateCheckIntervalMinutes_InRange_PassesValidation(int validValue)
    {
        var settings = new AppSettings { UpdateCheckIntervalMinutes = validValue };
        var context = new ValidationContext(settings);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(settings, context, results, validateAllProperties: true)
            .Should().BeTrue();
    }
}

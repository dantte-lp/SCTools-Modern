// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using SCTools.Core.Models;
using SCTools.Core.ViewModels;
using Xunit;

namespace SCTools.Tests.ViewModels;

public sealed class SettingsViewModelTests
{
    private readonly SettingsViewModel _sut = new();

    // --- LoadFrom ---
    [Fact]
    public void LoadFrom_SetsAllProperties()
    {
        var settings = CreateTestSettings();

        _sut.LoadFrom(settings);

        _sut.GameFolder.Should().Be("/game/StarCitizen");
        _sut.RunMinimized.Should().BeTrue();
        _sut.AllowIncrementalDownload.Should().BeFalse();
        _sut.RegularCheckForUpdates.Should().BeFalse();
        _sut.UpdateCheckIntervalMinutes.Should().Be(30);
    }

    [Fact]
    public void LoadFrom_ThrowsOnNull()
    {
        var act = () => _sut.LoadFrom(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void LoadFrom_ResetsHasChanges()
    {
        _sut.LoadFrom(CreateTestSettings());
        _sut.GameFolder = "/different/path";
        _sut.HasChanges.Should().BeTrue();

        _sut.LoadFrom(CreateTestSettings());

        _sut.HasChanges.Should().BeFalse();
    }

    // --- ToSettings ---
    [Fact]
    public void ToSettings_ReturnsCurrentValues()
    {
        _sut.GameFolder = "/my/game";
        _sut.RunMinimized = true;
        _sut.AllowIncrementalDownload = false;
        _sut.RegularCheckForUpdates = false;
        _sut.UpdateCheckIntervalMinutes = 15;

        var result = _sut.ToSettings();

        result.GameFolder.Should().Be("/my/game");
        result.RunMinimized.Should().BeTrue();
        result.AllowIncrementalDownload.Should().BeFalse();
        result.RegularCheckForUpdates.Should().BeFalse();
        result.UpdateCheckIntervalMinutes.Should().Be(15);
    }

    // --- HasChanges ---
    [Fact]
    public void HasChanges_InitiallyFalse()
    {
        _sut.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void HasChanges_TrueWhenGameFolderDiffers()
    {
        _sut.LoadFrom(new AppSettings { GameFolder = "/original" });

        _sut.GameFolder = "/changed";

        _sut.HasChanges.Should().BeTrue();
    }

    [Fact]
    public void HasChanges_TrueWhenRunMinimizedDiffers()
    {
        _sut.LoadFrom(new AppSettings());

        _sut.RunMinimized = true;

        _sut.HasChanges.Should().BeTrue();
    }

    [Fact]
    public void HasChanges_TrueWhenIntervalDiffers()
    {
        _sut.LoadFrom(new AppSettings { UpdateCheckIntervalMinutes = 60 });

        _sut.UpdateCheckIntervalMinutes = 120;

        _sut.HasChanges.Should().BeTrue();
    }

    [Fact]
    public void HasChanges_FalseWhenRevertedToOriginal()
    {
        _sut.LoadFrom(new AppSettings { GameFolder = "/original" });
        _sut.GameFolder = "/changed";
        _sut.HasChanges.Should().BeTrue();

        _sut.GameFolder = "/original";

        _sut.HasChanges.Should().BeFalse();
    }

    // --- Revert ---
    [Fact]
    public void Revert_RestoresOriginalValues()
    {
        var original = CreateTestSettings();
        _sut.LoadFrom(original);
        _sut.GameFolder = "/changed";
        _sut.RunMinimized = false;
        _sut.UpdateCheckIntervalMinutes = 999;

        _sut.RevertCommand.Execute(null);

        _sut.GameFolder.Should().Be(original.GameFolder);
        _sut.RunMinimized.Should().Be(original.RunMinimized);
        _sut.UpdateCheckIntervalMinutes.Should().Be(original.UpdateCheckIntervalMinutes);
        _sut.HasChanges.Should().BeFalse();
    }

    // --- Property change notifications ---
    [Fact]
    public void PropertyChanged_FiredForHasChanges()
    {
        _sut.LoadFrom(new AppSettings());
        var changedProperties = new List<string>();
        _sut.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        _sut.GameFolder = "/new";

        changedProperties.Should().Contain(nameof(SettingsViewModel.HasChanges));
    }

    [Fact]
    public void PropertyChanged_FiredForAllSettingsProperties()
    {
        var changedProperties = new List<string>();
        _sut.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        _sut.GameFolder = "/path";
        _sut.RunMinimized = true;
        _sut.AllowIncrementalDownload = false;
        _sut.RegularCheckForUpdates = false;
        _sut.UpdateCheckIntervalMinutes = 42;

        changedProperties.Should().Contain(nameof(SettingsViewModel.GameFolder));
        changedProperties.Should().Contain(nameof(SettingsViewModel.RunMinimized));
        changedProperties.Should().Contain(nameof(SettingsViewModel.AllowIncrementalDownload));
        changedProperties.Should().Contain(nameof(SettingsViewModel.RegularCheckForUpdates));
        changedProperties.Should().Contain(nameof(SettingsViewModel.UpdateCheckIntervalMinutes));
    }

    private static AppSettings CreateTestSettings() => new()
    {
        GameFolder = "/game/StarCitizen",
        RunMinimized = true,
        AllowIncrementalDownload = false,
        RegularCheckForUpdates = false,
        UpdateCheckIntervalMinutes = 30,
    };
}

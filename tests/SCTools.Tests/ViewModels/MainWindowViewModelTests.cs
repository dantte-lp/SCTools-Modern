// Licensed to the SCTools project under the MIT license.

using FluentAssertions;
using NSubstitute;
using SCTools.Core.Models;
using SCTools.Core.Services.Interfaces;
using SCTools.Core.ViewModels;
using Xunit;

namespace SCTools.Tests.ViewModels;

public sealed class MainWindowViewModelTests
{
    private readonly IGameDetectionService _gameDetection = Substitute.For<IGameDetectionService>();
    private readonly IAutoUpdateService _autoUpdate = Substitute.For<IAutoUpdateService>();
    private readonly MainWindowViewModel _sut;

    public MainWindowViewModelTests()
    {
        _autoUpdate.CurrentVersion.Returns("1.0.0");
        _sut = new MainWindowViewModel(_gameDetection, _autoUpdate);
    }

    // --- Constructor ---
    [Fact]
    public void Constructor_SetsAppVersionFromAutoUpdate()
    {
        _sut.AppVersion.Should().Be("1.0.0");
    }

    [Fact]
    public void Constructor_DefaultGameModeIsLive()
    {
        _sut.SelectedGameMode.Should().Be(GameMode.Live);
    }

    [Fact]
    public void AvailableGameModes_ContainsAllValues()
    {
        _sut.AvailableGameModes.Should().BeEquivalentTo(Enum.GetValues<GameMode>());
    }

    // --- Initialize ---
    [Fact]
    public void Initialize_WithGameFolder_DetectsInstallations()
    {
        var installations = new List<GameInstallation>
        {
            new() { Mode = GameMode.Live, RootFolderPath = "/game/LIVE", ExeFilePath = "/game/LIVE/Bin64/StarCitizen.exe" },
        };
        _gameDetection.DetectInstallations("/game").Returns(installations);

        _sut.Initialize(new AppSettings { GameFolder = "/game" });

        _sut.GameFolder.Should().Be("/game");
        _sut.Installations.Should().HaveCount(1);
    }

    [Fact]
    public void Initialize_WithNullGameFolder_DoesNotDetect()
    {
        _sut.Initialize(new AppSettings { GameFolder = null });

        _sut.Installations.Should().BeEmpty();
        _gameDetection.DidNotReceive().DetectInstallations(Arg.Any<string>());
    }

    [Fact]
    public void Initialize_ThrowsOnNull()
    {
        var act = () => _sut.Initialize(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // --- DetectInstallations ---
    [Fact]
    public void DetectInstallations_PopulatesInstallations()
    {
        _sut.GameFolder = "/game";
        var installations = new List<GameInstallation>
        {
            new() { Mode = GameMode.Live, RootFolderPath = "/game/LIVE", ExeFilePath = "/game/LIVE/Bin64/StarCitizen.exe" },
            new() { Mode = GameMode.Ptu, RootFolderPath = "/game/PTU", ExeFilePath = "/game/PTU/Bin64/StarCitizen.exe" },
        };
        _gameDetection.DetectInstallations("/game").Returns(installations);

        _sut.DetectInstallationsCommand.Execute(null);

        _sut.Installations.Should().HaveCount(2);
        _sut.StatusMessage.Should().Contain("2 installation(s)");
    }

    [Fact]
    public void DetectInstallations_WhenNoFolder_SetsStatusMessage()
    {
        _sut.GameFolder = null;

        _sut.DetectInstallationsCommand.Execute(null);

        _sut.StatusMessage.Should().Be("Game folder not set.");
        _sut.Installations.Should().BeEmpty();
    }

    [Fact]
    public void DetectInstallations_WhenNoInstallationsFound_SetsStatusMessage()
    {
        _sut.GameFolder = "/game";
        _gameDetection.DetectInstallations("/game").Returns(new List<GameInstallation>());

        _sut.DetectInstallationsCommand.Execute(null);

        _sut.StatusMessage.Should().Be("No installations detected.");
    }

    [Fact]
    public void DetectInstallations_SelectsMatchingGameMode()
    {
        _sut.GameFolder = "/game";
        _sut.SelectedGameMode = GameMode.Ptu;
        var installations = new List<GameInstallation>
        {
            new() { Mode = GameMode.Live, RootFolderPath = "/game/LIVE", ExeFilePath = "/game/LIVE/Bin64/StarCitizen.exe" },
            new() { Mode = GameMode.Ptu, RootFolderPath = "/game/PTU", ExeFilePath = "/game/PTU/Bin64/StarCitizen.exe" },
        };
        _gameDetection.DetectInstallations("/game").Returns(installations);

        _sut.DetectInstallationsCommand.Execute(null);

        _sut.SelectedInstallation!.Mode.Should().Be(GameMode.Ptu);
    }

    [Fact]
    public void DetectInstallations_FallsBackToFirstInstallation()
    {
        _sut.GameFolder = "/game";
        _sut.SelectedGameMode = GameMode.Eptu;
        var installations = new List<GameInstallation>
        {
            new() { Mode = GameMode.Live, RootFolderPath = "/game/LIVE", ExeFilePath = "/game/LIVE/Bin64/StarCitizen.exe" },
        };
        _gameDetection.DetectInstallations("/game").Returns(installations);

        _sut.DetectInstallationsCommand.Execute(null);

        _sut.SelectedInstallation!.Mode.Should().Be(GameMode.Live);
    }

    // --- SelectedGameMode ---
    [Fact]
    public void SelectedGameModeChanged_UpdatesSelectedInstallation()
    {
        _sut.GameFolder = "/game";
        var installations = new List<GameInstallation>
        {
            new() { Mode = GameMode.Live, RootFolderPath = "/game/LIVE", ExeFilePath = "/game/LIVE/Bin64/StarCitizen.exe" },
            new() { Mode = GameMode.Ptu, RootFolderPath = "/game/PTU", ExeFilePath = "/game/PTU/Bin64/StarCitizen.exe" },
        };
        _gameDetection.DetectInstallations("/game").Returns(installations);
        _sut.DetectInstallationsCommand.Execute(null);

        _sut.SelectedGameMode = GameMode.Ptu;

        _sut.SelectedInstallation!.Mode.Should().Be(GameMode.Ptu);
    }

    // --- SelectedGameModePath ---
    [Fact]
    public void SelectedGameModePath_ReflectsSelectedInstallation()
    {
        _sut.GameFolder = "/game";
        var installations = new List<GameInstallation>
        {
            new() { Mode = GameMode.Live, RootFolderPath = "/game/LIVE", ExeFilePath = "/game/LIVE/Bin64/StarCitizen.exe" },
        };
        _gameDetection.DetectInstallations("/game").Returns(installations);
        _sut.DetectInstallationsCommand.Execute(null);

        _sut.SelectedGameModePath.Should().Be("/game/LIVE");
    }

    [Fact]
    public void SelectedGameModePath_NullWhenNoInstallation()
    {
        _sut.SelectedGameModePath.Should().BeNull();
    }

    // --- CheckForAppUpdateAsync ---
    [Fact]
    public async Task CheckForAppUpdateAsync_WhenUpdateAvailable_SetsAvailableUpdate()
    {
        var update = new AppUpdateInfo { TargetVersion = "2.0.0", CurrentVersion = "1.0.0" };
        _autoUpdate.CheckForUpdateAsync(Arg.Any<CancellationToken>()).Returns(update);

        await _sut.CheckForAppUpdateCommand.ExecuteAsync(null);

        _sut.AvailableUpdate.Should().Be(update);
        _sut.StatusMessage.Should().Contain("2.0.0");
        _sut.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task CheckForAppUpdateAsync_WhenUpToDate_ClearsAvailableUpdate()
    {
        _autoUpdate.CheckForUpdateAsync(Arg.Any<CancellationToken>()).Returns((AppUpdateInfo?)null);

        await _sut.CheckForAppUpdateCommand.ExecuteAsync(null);

        _sut.AvailableUpdate.Should().BeNull();
        _sut.StatusMessage.Should().Be("Application is up to date.");
    }

    // --- Property change notifications ---
    [Fact]
    public void PropertyChanged_FiredForSelectedGameMode()
    {
        var changedProperties = new List<string>();
        _sut.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        _sut.SelectedGameMode = GameMode.Ptu;

        changedProperties.Should().Contain(nameof(MainWindowViewModel.SelectedGameMode));
    }

    [Fact]
    public void PropertyChanged_FiredForGameFolder()
    {
        var changedProperties = new List<string>();
        _sut.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        _sut.GameFolder = "/new/path";

        changedProperties.Should().Contain(nameof(MainWindowViewModel.GameFolder));
    }
}

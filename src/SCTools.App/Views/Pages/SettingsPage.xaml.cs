// Licensed to the SCTools project under the MIT license.

using System.Windows;
using Microsoft.Win32;
using SCTools.Core.ViewModels;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace SCTools.App.Views.Pages;

/// <summary>
/// Settings page for configuring application preferences.
/// </summary>
public partial class SettingsPage : INavigableView<SettingsViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsPage"/> class.
    /// </summary>
    /// <param name="viewModel">Settings view model (from Core).</param>
    /// <param name="mainViewModel">Main window view model for version info.</param>
    public SettingsPage(SettingsViewModel viewModel, MainWindowViewModel mainViewModel)
    {
        ArgumentNullException.ThrowIfNull(mainViewModel);

        ViewModel = viewModel;
        AppVersion = mainViewModel.AppVersion;
        DataContext = this;

        InitializeComponent();
    }

    /// <summary>
    /// Gets the .NET runtime version string.
    /// </summary>
    public static string DotNetVersion => Environment.Version.ToString();

    /// <inheritdoc />
    public SettingsViewModel ViewModel { get; }

    /// <summary>
    /// Gets the current application version string.
    /// </summary>
    public string? AppVersion { get; }

    private void BrowseGameFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Star Citizen game folder",
        };

        if (dialog.ShowDialog() == true)
        {
            ViewModel.GameFolder = dialog.FolderName;
        }
    }
}

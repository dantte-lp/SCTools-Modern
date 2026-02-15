// Licensed to the SCTools project under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using SCTools.Core.ViewModels;

namespace SCTools.App.ViewModels;

/// <summary>
/// Thin shell ViewModel for the main window.
/// Composes the Core <see cref="Core.ViewModels.MainWindowViewModel"/> with UI-specific properties.
/// </summary>
public partial class ShellViewModel : ObservableObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">Core main window ViewModel.</param>
    public ShellViewModel(MainWindowViewModel mainViewModel)
    {
        Main = mainViewModel;
    }

    /// <summary>Gets the Core main window ViewModel.</summary>
    public MainWindowViewModel Main { get; }

    /// <summary>Gets the application title displayed in the title bar.</summary>
    public string ApplicationTitle { get; } = "SCTools";
}

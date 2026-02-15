// Licensed to the SCTools project under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;

namespace SCTools.App.ViewModels;

/// <summary>
/// ViewModel for the main application window.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the application title displayed in the title bar.
    /// </summary>
    [ObservableProperty]
    private string _applicationTitle = "SCTools";
}

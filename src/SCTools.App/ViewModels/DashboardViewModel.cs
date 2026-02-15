// Licensed to the SCTools project under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;

namespace SCTools.App.ViewModels;

/// <summary>
/// ViewModel for the dashboard page.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the status message displayed on the dashboard.
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "Star Citizen companion tool \u2014 ready.";
}

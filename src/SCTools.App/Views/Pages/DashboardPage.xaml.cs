// Licensed to the SCTools project under the MIT license.

using SCTools.App.ViewModels;
using Wpf.Ui.Controls;

namespace SCTools.App.Views.Pages;

/// <summary>
/// Dashboard page showing application status and quick actions.
/// </summary>
public partial class DashboardPage : INavigableView<DashboardViewModel>
{
    /// <inheritdoc />
    public DashboardViewModel ViewModel { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardPage"/> class.
    /// </summary>
    /// <param name="viewModel">Dashboard view model.</param>
    public DashboardPage(DashboardViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}

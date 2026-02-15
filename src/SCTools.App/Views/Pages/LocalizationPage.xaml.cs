// Licensed to the SCTools project under the MIT license.

using SCTools.Core.ViewModels;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace SCTools.App.Views.Pages;

/// <summary>
/// Localization management page — install, update, and manage language packs.
/// </summary>
public partial class LocalizationPage : INavigableView<LocalizationViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationPage"/> class.
    /// </summary>
    /// <param name="viewModel">Localization view model (from Core).</param>
    /// <param name="mainViewModel">Main window view model for game mode path binding.</param>
    public LocalizationPage(LocalizationViewModel viewModel, MainWindowViewModel mainViewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(mainViewModel);

        ViewModel = viewModel;
        DataContext = this;

        // Sync game mode path from MainWindowViewModel
        viewModel.GameModePath = mainViewModel.SelectedGameModePath;
        mainViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.SelectedGameModePath))
            {
                viewModel.GameModePath = mainViewModel.SelectedGameModePath;
            }
        };

        InitializeComponent();
    }

    /// <inheritdoc />
    public LocalizationViewModel ViewModel { get; }
}

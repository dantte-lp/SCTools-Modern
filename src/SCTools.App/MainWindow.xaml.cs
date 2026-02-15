// Licensed to the SCTools project under the MIT license.

using SCTools.App.ViewModels;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace SCTools.App;

/// <summary>
/// Main application window with Fluent Design and sidebar navigation.
/// </summary>
public partial class MainWindow : INavigationWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <param name="navigationService">Navigation service for page routing.</param>
    /// <param name="pageProvider">Page provider for DI-based page creation.</param>
    /// <param name="viewModel">Main window view model.</param>
    public MainWindow(
        INavigationService navigationService,
        INavigationViewPageProvider pageProvider,
        MainWindowViewModel viewModel)
    {
        DataContext = viewModel;

        InitializeComponent();

        navigationService.SetNavigationControl(RootNavigation);
        RootNavigation.SetPageProviderService(pageProvider);

        SystemThemeWatcher.Watch(this);
    }

    /// <inheritdoc />
    public INavigationView GetNavigation() => RootNavigation;

    /// <inheritdoc />
    public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

    /// <inheritdoc />
    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        // Service provider is injected via constructor; no additional setup required.
    }

    /// <inheritdoc />
    public void SetPageService(INavigationViewPageProvider pageProvider)
    {
        RootNavigation.SetPageProviderService(pageProvider);
    }

    /// <inheritdoc />
    public void ShowWindow() => Show();

    /// <inheritdoc />
    public void CloseWindow() => Close();
}

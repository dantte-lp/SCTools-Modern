// Licensed to the SCTools project under the MIT license.

using System.ComponentModel;
using System.Windows;
using SCTools.App.ViewModels;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace SCTools.App;

/// <summary>
/// Main application window with Fluent Design, sidebar navigation, and system tray.
/// </summary>
public partial class MainWindow : INavigationWindow
{
    private readonly ISnackbarService _snackbarService;
    private readonly IContentDialogService _contentDialogService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <param name="navigationService">Navigation service for page routing.</param>
    /// <param name="navigationViewPageProvider">Page provider for DI-based page creation.</param>
    /// <param name="viewModel">Shell view model.</param>
    /// <param name="snackbarService">Snackbar notification service.</param>
    /// <param name="contentDialogService">Content dialog service.</param>
    public MainWindow(
        INavigationService navigationService,
        INavigationViewPageProvider navigationViewPageProvider,
        ShellViewModel viewModel,
        ISnackbarService snackbarService,
        IContentDialogService contentDialogService)
    {
        ArgumentNullException.ThrowIfNull(navigationService);

        _snackbarService = snackbarService;
        _contentDialogService = contentDialogService;

        DataContext = viewModel;

        InitializeComponent();

        // Wire WPF UI services
        navigationService.SetNavigationControl(RootNavigation);
        RootNavigation.SetPageProviderService(navigationViewPageProvider);
        _snackbarService.SetSnackbarPresenter(RootSnackbarPresenter);
        _contentDialogService.SetDialogHost(RootContentDialogPresenter);

        // Follow system theme
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
    public void SetPageService(INavigationViewPageProvider navigationViewPageProvider)
    {
        RootNavigation.SetPageProviderService(navigationViewPageProvider);
    }

    /// <inheritdoc />
    public void ShowWindow() => Show();

    /// <inheritdoc />
    public void CloseWindow() => Close();

    /// <inheritdoc />
    protected override void OnClosing(CancelEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        // Minimize to tray instead of closing
        e.Cancel = true;
        Hide();
    }

    private void TrayIcon_OnTrayLeftMouseDown(object sender, RoutedEventArgs e)
    {
        ShowAndActivate();
    }

    private void TrayMenu_Show_Click(object sender, RoutedEventArgs e)
    {
        ShowAndActivate();
    }

    private void TrayMenu_Exit_Click(object sender, RoutedEventArgs e)
    {
        TrayIcon.Dispose();
        Application.Current.Shutdown();
    }

    private void ShowAndActivate()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }
}

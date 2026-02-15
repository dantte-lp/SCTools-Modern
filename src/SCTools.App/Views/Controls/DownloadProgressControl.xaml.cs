// Licensed to the SCTools project under the MIT license.

using System.Windows.Controls;

namespace SCTools.App.Views.Controls;

/// <summary>
/// User control for displaying download progress.
/// Intended to be used inside a <see cref="Wpf.Ui.Controls.ContentDialog"/>.
/// DataContext should be a <see cref="Core.ViewModels.DownloadProgressViewModel"/>.
/// </summary>
public partial class DownloadProgressControl : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadProgressControl"/> class.
    /// </summary>
    public DownloadProgressControl()
    {
        InitializeComponent();
    }
}

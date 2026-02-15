// Licensed to the SCTools project under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SCTools.Core.Models;

namespace SCTools.Core.ViewModels;

/// <summary>
/// ViewModel for tracking download progress.
/// Implements <see cref="IProgress{DownloadProgressInfo}"/> so it can be passed
/// directly to service methods that report download progress.
/// </summary>
public sealed partial class DownloadProgressViewModel : ObservableObject, IProgress<DownloadProgressInfo>, IDisposable
{
    private CancellationTokenSource? _cts;
    private bool _disposed;

    /// <summary>Gets or sets the total number of bytes to download, or <c>null</c> if unknown.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressPercent))]
    [NotifyPropertyChangedFor(nameof(IsIndeterminate))]
    private long? _totalBytes;

    /// <summary>Gets or sets the number of bytes downloaded so far.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressPercent))]
    private long _bytesDownloaded;

    /// <summary>Gets or sets the name of the current file being processed.</summary>
    [ObservableProperty]
    private string? _currentFile;

    /// <summary>Gets or sets a value indicating whether a download is active.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool _isDownloading;

    /// <summary>Gets or sets the title describing the current operation.</summary>
    [ObservableProperty]
    private string _title = string.Empty;

    /// <summary>
    /// Gets the download progress as a percentage (0-100), or 0 if the total size is unknown.
    /// </summary>
    public double ProgressPercent =>
        TotalBytes is > 0 ? (double)BytesDownloaded / TotalBytes.Value * 100.0 : 0;

    /// <summary>
    /// Gets a value indicating whether the total download size is unknown (indeterminate progress bar).
    /// </summary>
    public bool IsIndeterminate => TotalBytes is null or 0;

    /// <summary>
    /// Starts tracking a new download operation.
    /// </summary>
    /// <param name="title">Title describing the operation.</param>
    /// <returns>A <see cref="CancellationToken"/> that is cancelled when the user requests cancellation.</returns>
    public CancellationToken Start(string title)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        Title = title;
        IsDownloading = true;
        BytesDownloaded = 0;
        TotalBytes = null;
        CurrentFile = null;

        return _cts.Token;
    }

    /// <summary>
    /// Marks the current download operation as complete.
    /// </summary>
    public void Complete()
    {
        IsDownloading = false;
    }

    /// <inheritdoc />
    public void Report(DownloadProgressInfo value)
    {
        ArgumentNullException.ThrowIfNull(value);

        TotalBytes = value.TotalBytes;
        BytesDownloaded = value.BytesDownloaded;
        CurrentFile = value.CurrentFile;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _cts?.Dispose();
            _disposed = true;
        }
    }

    /// <summary>
    /// Cancels the active download operation.
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsDownloading))]
    private void Cancel()
    {
        _cts?.Cancel();
    }
}

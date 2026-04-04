using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Interface;

abstract class MasterDialog
{
    protected MasterDialog() { }

    static readonly SemaphoreSlim s_semaphore = new(1, 1);

    internal static ContentDialog Current => field ??= new();

    protected abstract string Title { get; }
    protected abstract string Content { get; }
    protected abstract string PrimaryButtonText { get; }
    protected virtual string CloseButtonText { get; } = string.Empty;
    protected virtual string SecondaryButtonText { get; } = string.Empty;

    internal virtual async Task<bool> ShowAsync() => await PromptAsync() != ContentDialogResult.None;

    internal async Task<ContentDialogResult> PromptAsync()
    {
        await s_semaphore.WaitAsync();
        try
        {
            Current.Title = Title;
            Current.Content = Content;

            Current.CloseButtonText = CloseButtonText;
            Current.PrimaryButtonText = PrimaryButtonText;
            Current.SecondaryButtonText = SecondaryButtonText;

            return await Current.ShowAsync();
        }
        finally { s_semaphore.Release(); }
    }
}
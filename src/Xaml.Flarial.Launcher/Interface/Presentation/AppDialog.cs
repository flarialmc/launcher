using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Vpn;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Interface.Presentation;

abstract class AppDialog<T> : AppDialog where T : AppDialog<T>, new()
{
    internal static readonly T _ = new();
}

abstract class AppDialog
{
    protected AppDialog() { }

    static readonly SemaphoreSlim s_semaphore = new(1, 1);

    internal static ContentDialog Current => field ??= new();

    protected abstract string Title { get; }
    protected abstract string Content { get; }
    protected abstract string PrimaryButtonText { get; }

    protected virtual string CloseButtonText { get; } = string.Empty;
    protected virtual string SecondaryButtonText { get; } = string.Empty;

    protected virtual async Task OnShowAsync(bool value) { }

    internal async Task<bool> ShowAsync()
    {
        await s_semaphore.WaitAsync();
        try
        {
            Current.Title = Title;
            Current.Content = Content;

            Current.CloseButtonText = CloseButtonText;
            Current.PrimaryButtonText = PrimaryButtonText;
            Current.SecondaryButtonText = SecondaryButtonText;

            var value = await Current.ShowAsync() > 0;
            await OnShowAsync(value); return value;
        }
        finally { s_semaphore.Release(); }
    }
}
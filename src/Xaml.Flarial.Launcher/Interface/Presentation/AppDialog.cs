using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Interface.Presentation;

abstract class AppDialog<T> : AppDialog where T : AppDialog<T>, new()
{
    static readonly ConcurrentDictionary<Type, AppDialog<T>> s_dialogs = [];

    static AppDialog<T> Get() => s_dialogs.GetOrAdd(typeof(T), static type => new T());

    public static async Task<bool> ShowAsync() => await Get().OnShowAsync();
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

    internal virtual async Task<bool> OnShowAsync()
    {
        await s_semaphore.WaitAsync();
        try
        {
            Current.Title = Title;
            Current.Content = Content;

            Current.CloseButtonText = CloseButtonText;
            Current.PrimaryButtonText = PrimaryButtonText;
            Current.SecondaryButtonText = SecondaryButtonText;

            return (await Current.ShowAsync()) > 0;
        }
        finally { s_semaphore.Release(); }
    }
}
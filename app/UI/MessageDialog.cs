using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ModernWpf.Controls;

namespace Flarial.Launcher.UI;

static class MessageDialog
{
    static readonly ContentDialog s_dialog = new();
    static readonly SemaphoreSlim s_semaphore = new(1, 1);

    public static bool IsShown => s_semaphore.CurrentCount <= 0;

    internal static async Task<bool> ShowAsync(string title, string content, string primary, [Optional] string? close)
    {
        await s_semaphore.WaitAsync(); try
        {
            s_dialog.Title = title;
            s_dialog.Content = content;

            s_dialog.CloseButtonText = close;
            s_dialog.PrimaryButtonText = primary;

            return await s_dialog.ShowAsync() != ContentDialogResult.None;
        }
        finally { s_semaphore.Release(); }
    }

    internal static async Task<bool> ShowAsync(MessageDialogContent content)
    {
        return await ShowAsync(content.Title, content.Content, content.Primary, content.Close);
    }
}
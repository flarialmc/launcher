using System;
using System.Threading;
using System.Threading.Tasks;
using Flarial.Launcher.Interface.Dialogs;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Interface;

abstract class MainDialog
{
    protected MainDialog() { }
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

    internal static NotInstalledDialog NotInstalled => field ??= new();
    internal static LaunchFailureDialog LaunchFailure => field ??= new();
    internal static SelectVersionDialog SelectVersion => field ??= new();
    internal static UWPDeprecatedDialog UWPDeprecated => field ??= new();
    internal static InstallVersionDialog InstallVersion => field ??= new();
    internal static UnsignedInstallDialog UnsignedInstall => field ??= new();
    internal static InvalidCustomDllDialog InvalidCustomDll => field ??= new();
    internal static ConnectionFailureDialog ConnectionFailure => field ??= new();
    internal static UnpackagedInstallDialog UnpackagedInstall => field ??= new();
    internal static ClientUpdateFailureDialog ClientUpdateFailure => field ??= new();
    internal static AllowUnsignedInstallDialog AllowUnsignedInstall => field ??= new();
    internal static GamingServicesMissingDialog GamingServicesMissing => field ??= new();
    internal static LauncherUpdateAvailableDialog LauncherUpdateAvailable => field ??= new();
}
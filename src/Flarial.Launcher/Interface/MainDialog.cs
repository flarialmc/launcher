using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flarial.Launcher.Interface.Dialogs;
using Flarial.Launcher.Management;
using Flarial.Launcher.Runtime.Versions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Interface;

abstract class MainDialog
{
    internal MainDialog() { }

    readonly ContentDialog _dialog = new();
    static readonly SemaphoreSlim s_semaphore = new(1, 1);

    protected abstract string Title { get; }
    protected abstract string Content { get; }
    protected abstract string PrimaryButtonText { get; }
    protected virtual string CloseButtonText { get; } = string.Empty;
    protected virtual string SecondaryButtonText { get; } = string.Empty;

    internal virtual async Task<bool> ShowAsync(UIElement element) => await PromptAsync(element) != ContentDialogResult.None;

    internal async Task<ContentDialogResult> PromptAsync(UIElement element)
    {
        await s_semaphore.WaitAsync();
        try
        {
            _dialog.Title = Title;
            _dialog.Content = Content;

            _dialog.CloseButtonText = CloseButtonText;
            _dialog.PrimaryButtonText = PrimaryButtonText;
            _dialog.SecondaryButtonText = SecondaryButtonText;

            _dialog.XamlRoot = element.XamlRoot;

            return await _dialog.ShowAsync();
        }
        finally { s_semaphore.Release(); }
    }

    internal static BetaDllUsageDialog BetaDllUsage => field ??= new();
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
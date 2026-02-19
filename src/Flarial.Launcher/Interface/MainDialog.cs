using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

    internal static BetaDllUsage BetaDllUsage => field ??= new();
    internal static NotInstalled NotInstalled => field ??= new();
    internal static LaunchFailure LaunchFailure => field ??= new();
    internal static SelectVersion SelectVersion => field ??= new();
    internal static UWPDeprecated UWPDeprecated => field ??= new();
    internal static InstallVersion InstallVersion => field ??= new();
    internal static UnsignedInstall UnsignedInstall => field ??= new();
    internal static InvalidCustomDll InvalidCustomDll => field ??= new();
    internal static ConnectionFailure ConnectionFailure => field ??= new();
    internal static UnpackagedInstall UnpackagedInstall => field ??= new();
    internal static ClientUpdateFailure ClientUpdateFailure => field ??= new();
    internal static AllowUnsignedInstall AllowUnsignedInstall => field ??= new();
    internal static GamingServicesMissing GamingServicesMissing => field ??= new();
    internal static LauncherUpdateAvailable LauncherUpdateAvailable => field ??= new();
}

sealed class UnsupportedVersion(string preferred) : MainDialog
{
    protected override string CloseButtonText => "Back";
    protected override string PrimaryButtonText => "Versions";
    protected override string SecondaryButtonText => "Settings";
    protected override string Title => "⚠️ Unsupported Version";

    protected override string Content
    {
        get
        {
            var key = VersionRegistry.InstalledVersion;

            if (_cache.TryGetValue(key, out var value))
                return value;

            value = string.Format(_format, key);
            _cache.Add(key, value);

            return value;
        }
    }

    readonly Dictionary<string, string> _cache = [];

    readonly string _format = $@"Minecraft {{0}} isn't supported by Flarial Client.

• Switch to {preferred} on the [Versions] page.
• Enable the client's beta on the [Settings] page.

If you need help, join our Discord.";
}

sealed class UWPDeprecated : MainDialog
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "⚠️ UWP Deprecated";
    protected override string Content => @"The client & launcher no longer support UWP builds.

• UWP builds are now outdated & deprecated.
• Consider updating to the latest GDK build of the game.

If you need help, join our Discord.";
}

sealed class AllowUnsignedInstall : UnsignedInstall
{
    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Launch";
}

class UnsignedInstall : MainDialog
{
    protected override string Title => "⚠️ Unsigned Install";
    protected override string PrimaryButtonText => "Back";
    protected override string Content => @"The launcher cannot verify the integrity of the game.

• Compatibility issues might arise with the client & launcher.
• Consider reinstalling the game via the Microsoft Store or Xbox App.

If you need help, join our Discord.";
}

sealed class GamingServicesMissing : MainDialog
{
    internal override async Task<bool> ShowAsync(UIElement element)
    {
        var _ = await base.ShowAsync(element);
        if (_) MicrosoftStorePage.GamingServices.Open();
        return _;
    }

    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Install";
    protected override string Title => "⚠️ Gaming Services Missing";
    protected override string Content => @"Gaming Services isn't installed, please install it.

• Gaming Services is required for GDK builds.
• You may install Gaming Services via the Microsoft Store.

If you need help, join our Discord.";
}

sealed class SelectVersion : MainDialog
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "💡 Select Version";
    protected override string Content => @"No Minecraft version is selected.

• Select a Minecraft version from the list that should be installed.

If you need help, join our Discord.";
}

sealed class InstallVersion : MainDialog
{
    protected override string Title => "💡 Install Version";

    protected override string Content => @"The selected Minecraft version will be now installed.
Once the installation starts, you won't able to cancel it.

• Free up disk space before proceeding with the installation.
• A high speed internet connection is recommended for this.

If you need help, join our Discord.";

    protected override string PrimaryButtonText => "Install";

    protected override string CloseButtonText => "Cancel";
}

[Obsolete("", true)]
sealed class CannotFind : MainDialog
{
    protected override string Title => "⚠️ Cannot Find";

    protected override string Content => @"The client's folder cannot be found.

• Try launching the client at least once to generate its folder.

If you need help, join our Discord.";

    protected override string PrimaryButtonText => "Back";
}

sealed class NotInstalled : MainDialog
{
    internal override async Task<bool> ShowAsync(UIElement element)
    {
        var _ = await base.ShowAsync(element);
        if (_) MicrosoftStorePage.Minecraft.Open();
        return _;
    }

    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Install";
    protected override string Title => "⚠️ Not Installed";
    protected override string Content => @"Minecraft: Bedrock Edition isn't installed.

• Install Minecraft: Bedrock Edition via the Microsoft Store or Xbox App.

If you need help, join our Discord.";
}

sealed class ConnectionFailure : MainDialog
{
    protected override string PrimaryButtonText => "Exit";
    protected override string Title => "🚨 Connection Failure";
    protected override string Content => @"Failed to connect to Flarial Client Services.
        
• Try restarting the launcher.
• Check your internet connection.
• Change your system DNS for both IPv4 and IPv6.

If you need help, join our Discord.";
}

sealed class InvalidCustomDll : MainDialog
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "⚠️ Invalid Custom DLL";
    protected override string Content => @"The specified custom DLL is invalid.

• Specify a DLL that is valid and exists.
• If you didn't intend to use this feature, disable it.
• Ensure no security software is blocking the launcher.

If you need help, join our Discord.";
}

sealed class LaunchFailure : MainDialog
{
    protected override string Title => "⚠️ Launch Failure";
    protected override string PrimaryButtonText => "Back";
    protected override string Content => @"The launcher couldn't inject or initialize Minecraft correctly.

• Remove & disable any 3rd party mods or tools.
• Ensure no security software is blocking the launcher.
• Try closing Minecraft & launching it again via the launcher.

If you need help, join our Discord.";
}

sealed class ClientUpdateFailure : MainDialog
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "⚠️ Client Update Failure";
    protected override string Content => @"A client update couldn't be downloaded.

• Try closing Minecraft & click on [Play] to update the client.
• Try rebooting your machine & see if that resolves the issue.

If you need help, join our Discord.";
}

sealed class LauncherUpdateAvailable : MainDialog
{
    protected override string Title => "💡 Launcher Update Available";
    protected override string PrimaryButtonText => "Update";
    protected override string CloseButtonText => "Later";
    protected override string Content => @"An update is available for the launcher.

• Updating the launcher provides fixes & new features.
• New versions of the client & Minecraft might require a launcher update.

If you need help, join our Discord.";
}

sealed class BetaDllUsage : MainDialog
{
    protected override string Title => "⚠️ Beta DLL Usage";
    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Launch";
    protected override string Content => @"The beta DLL of the client might be potentially unstable. 

• Bugs & crashes might occur frequently during gameplay.
• The beta DLL is meant for reporting bugs & issues with the client.

Hence use at your own risk.";
}

sealed class UnpackagedInstall : MainDialog
{
    protected override string Title => "⚠️ Unpackaged Install";
    protected override string PrimaryButtonText => "Back";
    protected override string Content => @"The current Minecraft install is unpackaged.

• Please reinstall the game via the Microsoft or Xbox App.
• The launcher can only switch versions if the install is packaged.

If you need help, join our Discord.";
}
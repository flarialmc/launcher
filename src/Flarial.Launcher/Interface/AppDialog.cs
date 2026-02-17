using System;
using System.Threading;
using System.Threading.Tasks;
using Flarial.Launcher.Management;
using ModernWpf.Controls;

namespace Flarial.Launcher.Interface;

abstract class AppDialog
{
    internal AppDialog()
    {
        _dialog.Title = Title;
        _dialog.Content = Content;
        _dialog.CloseButtonText = CloseButtonText;
        _dialog.PrimaryButtonText = PrimaryButtonText;
        _dialog.SecondaryButtonText = SecondaryButtonText;
    }

    readonly ContentDialog _dialog = new();
    static readonly SemaphoreSlim s_semaphore = new(1, 1);

    protected abstract string Title { get; }
    protected abstract string Content { get; }
    protected virtual string? CloseButtonText { get; }
    protected abstract string PrimaryButtonText { get; }
    protected virtual string? SecondaryButtonText { get; }

    internal virtual async Task<bool> ShowAsync() => await PromptAsync() != ContentDialogResult.None;

    internal async Task<ContentDialogResult> PromptAsync()
    {
        await s_semaphore.WaitAsync();
        try { return await _dialog.ShowAsync(); }
        finally { s_semaphore.Release(); }
    }

    internal static readonly AppDialog BetaDllUsage = new BetaDllUsage();
    internal static readonly AppDialog NotInstalled = new NotInstalled();
    internal static readonly AppDialog LaunchFailure = new LaunchFailure();
    internal static readonly AppDialog SelectVersion = new SelectVersion();
    internal static readonly AppDialog UWPDeprecated = new UWPDeprecated();
    internal static readonly AppDialog InstallVersion = new InstallVersion();
    internal static readonly AppDialog UnsignedInstall = new UnsignedInstall();
    internal static readonly AppDialog InvalidCustomDll = new InvalidCustomDll();
    internal static readonly AppDialog ConnectionFailure = new ConnectionFailure();
    internal static readonly AppDialog UnpackagedInstall = new UnpackagedInstall();
    internal static readonly AppDialog ClientUpdateFailure = new ClientUpdateFailure();
    internal static readonly AppDialog AllowUnsignedInstall = new AllowUnsignedInstall();
    internal static readonly AppDialog GamingServicesMissing = new GamingServicesMissing();
    internal static readonly AppDialog LauncherUpdateAvailable = new LauncherUpdateAvailable();
}

file sealed class UWPDeprecated : AppDialog
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "⚠️ UWP Deprecated";
    protected override string Content => @"The client & launcher no longer support UWP builds.

• UWP builds are now outdated & deprecated.
• Consider updating to the latest GDK build of the game.

If you need help, join our Discord.";
}

file sealed class AllowUnsignedInstall : UnsignedInstall
{
    protected override string? CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Launch";
}

file class UnsignedInstall : AppDialog
{
    protected override string Title => "⚠️ Unsigned Install";
    protected override string PrimaryButtonText => "Back";
    protected override string Content => @"The launcher cannot verify the integrity of the game.

• Compatibility issues might arise with the client & launcher.
• Consider reinstalling the game via the Microsoft Store or Xbox App.

If you need help, join our Discord.";
}

file sealed class GamingServicesMissing : AppDialog
{
    internal override async Task<bool> ShowAsync()
    {
        var _ = await base.ShowAsync();
        if (_) Product.GamingServices.OpenProductDetailsPage();
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

file sealed class SelectVersion : AppDialog
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "💡 Select Version";
    protected override string Content => @"No Minecraft version is selected.

• Select a Minecraft version from the list that should be installed.

If you need help, join our Discord.";
}

file sealed class InstallVersion : AppDialog
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
file sealed class CannotFind : AppDialog
{
    protected override string Title => "⚠️ Cannot Find";

    protected override string Content => @"The client's folder cannot be found.

• Try launching the client at least once to generate its folder.

If you need help, join our Discord.";

    protected override string PrimaryButtonText => "Back";
}

file sealed class NotInstalled : AppDialog
{
    internal override async Task<bool> ShowAsync()
    {
        var _ = await base.ShowAsync();
        if (_) Product.Minecraft.OpenProductDetailsPage();
        return _;
    }

    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Install";
    protected override string Title => "⚠️ Not Installed";
    protected override string Content => @"Minecraft: Bedrock Edition isn't installed.

• Install Minecraft: Bedrock Edition via the Microsoft Store or Xbox App.

If you need help, join our Discord.";
}

file sealed class ConnectionFailure : AppDialog
{
    protected override string PrimaryButtonText => "Exit";
    protected override string Title => "🚨 Connection Failure";
    protected override string Content => @"Failed to connect to Flarial Client Services.
        
• Try restarting the launcher.
• Check your internet connection.
• Change your system DNS for both IPv4 and IPv6.

If you need help, join our Discord.";
}

file sealed class InvalidCustomDll : AppDialog
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "⚠️ Invalid Custom DLL";
    protected override string Content => @"The specified custom DLL is invalid.

• Specify a DLL that is valid and exists.
• If you didn't intend to use this feature, disable it.
• Ensure no security software is blocking the launcher.

If you need help, join our Discord.";
}

file sealed class LaunchFailure : AppDialog
{
    protected override string Title => "⚠️ Launch Failure";
    protected override string PrimaryButtonText => "Back";
    protected override string Content => @"The launcher couldn't inject or initialize Minecraft correctly.

• Remove & disable any 3rd party mods or tools.
• Ensure no security software is blocking the launcher.
• Try closing Minecraft & launching it again via the launcher.

If you need help, join our Discord.";
}

file sealed class ClientUpdateFailure : AppDialog
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "⚠️ Client Update Failure";
    protected override string Content => @"A client update couldn't be downloaded.

• Try closing Minecraft & click on [Play] to update the client.
• Try rebooting your machine & see if that resolves the issue.

If you need help, join our Discord.";
}

file sealed class LauncherUpdateAvailable : AppDialog
{
    protected override string Title => "💡 Launcher Update Available";
    protected override string PrimaryButtonText => "Update";
    protected override string CloseButtonText => "Later";
    protected override string Content => @"An update is available for the launcher.

• Updating the launcher provides fixes & new features.
• New versions of the client & Minecraft might require a launcher update.

If you need help, join our Discord.";
}

file sealed class BetaDllUsage : AppDialog
{
    protected override string Title => "⚠️ Beta DLL Usage";
    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Launch";
    protected override string Content => @"The beta DLL of the client might be potentially unstable. 

• Bugs & crashes might occur frequently during gameplay.
• The beta DLL is meant for reporting bugs & issues with the client.

Hence use at your own risk.";
}

file sealed class UnpackagedInstall : AppDialog
{
    protected override string Title => "⚠️ Unpackaged Install";
    protected override string PrimaryButtonText => "Back";
    protected override string Content => @"The current Minecraft install is unpackaged.

• Please reinstall the game via the Microsoft or Xbox App.
• The launcher can only switch versions if the install is packaged.

If you need help, join our Discord.";
}
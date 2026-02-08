using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Flarial.Launcher.Management;
using ModernWpf.Controls;

namespace Flarial.Launcher.Interface;

abstract class MainDialog
{
    internal MainDialog() { }
    static readonly SemaphoreSlim s_semaphore = new(1, 1);

    internal virtual async Task<bool> ShowAsync() => await PromptAsync() != ContentDialogResult.None;

    internal async Task<ContentDialogResult> PromptAsync()
    {
        await s_semaphore.WaitAsync(); try
        {
            await Dispatcher.Yield();
            return await new ContentDialog
            {
                Title = Title,
                Content = Content,
                CloseButtonText = CloseButtonText,
                PrimaryButtonText = PrimaryButtonText,
                SecondaryButtonText = SecondaryButtonText,
            }.ShowAsync(ContentDialogPlacement.InPlace);
        }
        finally { s_semaphore.Release(); }
    }

    protected abstract string Title { get; }
    protected abstract string Content { get; }
    protected abstract string PrimaryButtonText { get; }
    protected virtual string? CloseButtonText { get; }
    protected virtual string? SecondaryButtonText { get; }

    internal static readonly MainDialog NotInstalled = new NotInstalled();
    internal static readonly MainDialog LaunchFailure = new LaunchFailure();
    internal static readonly MainDialog SelectVersion = new SelectVersion();
    internal static readonly MainDialog BetaDllUsage = new BetaDllUsage();
    internal static readonly MainDialog InstallVersion = new InstallVersion();
    internal static readonly MainDialog InvalidCustomDll = new InvalidCustomDll();
    internal static readonly MainDialog ConnectionFailure = new ConnectionFailure();
    internal static readonly MainDialog ClientUpdateFailure = new ClientUpdateFailure();
    internal static readonly MainDialog UnpackagedInstall = new UnpackagedInstall();
    internal static readonly MainDialog GamingServicesMissing = new GamingServicesMissing();
    internal static readonly MainDialog LauncherUpdateAvailable = new LauncherUpdateAvailable();
}

file sealed class GamingServicesMissing : MainDialog
{
    internal override async Task<bool> ShowAsync()
    {
        var result = await base.ShowAsync();

        if (result)
            Product.GamingServices.OpenProductDetailsPage();

        return result;
    }

    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Install";
    protected override string Title => "⚠️ Gaming Services Missing";
    protected override string Content => @"Gaming Services isn't installed, please install it.

• Gaming Services is required for GDK builds.
• You may install Gaming Services via the Microsoft Store.

If you need help, join our Discord.";
}

file sealed class SelectVersion : MainDialog
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "💡 Select Version";
    protected override string Content => @"No Minecraft version is selected.

• Select a Minecraft version from the list that should be installed.

If you need help, join our Discord.";
}

file sealed class InstallVersion : MainDialog
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
file sealed class CannotFind : MainDialog
{
    protected override string Title => "⚠️ Cannot Find";

    protected override string Content => @"The client's folder cannot be found.

• Try launching the client at least once to generate its folder.

If you need help, join our Discord.";

    protected override string PrimaryButtonText => "Back";
}

file sealed class NotInstalled : MainDialog
{
    internal override async Task<bool> ShowAsync()
    {
        var result = await base.ShowAsync();

        if (result)
            Product.Minecraft.OpenProductDetailsPage();

        return result;
    }

    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Install";
    protected override string Title => "⚠️ Not Installed";
    protected override string Content => @"Minecraft: Bedrock Edition isn't installed.

• Install Minecraft: Bedrock Edition via the Microsoft Store or Xbox App.

If you need help, join our Discord.";
}

file sealed class ConnectionFailure : MainDialog
{
    protected override string PrimaryButtonText => "Exit";
    protected override string Title => "🚨 Connection Failure";
    protected override string Content => @"Failed to connect to Flarial Client Services.
        
• Try restarting the launcher.
• Check your internet connection.
• Change your system DNS for both IPv4 and IPv6.

If you need help, join our Discord.";
}

file sealed class InvalidCustomDll : MainDialog
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "⚠️ Invalid Custom DLL";
    protected override string Content => @"The specified custom DLL is invalid.

• Specify a DLL that is valid and exists.
• If you didn't intend to use this feature, disable it.
• Ensure no security software is blocking the launcher.

If you need help, join our Discord.";
}

file sealed class LaunchFailure : MainDialog
{
    protected override string Title => "⚠️ Launch Failure";
    protected override string PrimaryButtonText => "Back";
    protected override string Content => @"The launcher couldn't inject or initialize Minecraft correctly.

• Remove & disable any 3rd party mods or tools.
• Ensure no security software is blocking the launcher.
• Try closing Minecraft & launching it again via the launcher.

If you need help, join our Discord.";
}

file sealed class ClientUpdateFailure : MainDialog
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "⚠️ Client Update Failure";
    protected override string Content => @"A client update couldn't be downloaded.

• Try closing Minecraft & click on [Play] to update the client.
• Try rebooting your machine & see if that resolves the issue.

If you need help, join our Discord.";
}

file sealed class LauncherUpdateAvailable : MainDialog
{
    protected override string Title => "💡 Launcher Update Available";
    protected override string PrimaryButtonText => "Update";
    protected override string CloseButtonText => "Later";
    protected override string Content => @"An update is available for the launcher.

• Updating the launcher provides fixes & new features.
• New versions of the client & Minecraft might require a launcher update.

If you need help, join our Discord.";
}

file sealed class BetaDllUsage : MainDialog
{
    protected override string Title => "⚠️ Beta DLL Usage";
    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Launch";
    protected override string Content => @"The beta DLL of the client might be potentially unstable. 

• Bugs & crashes might occur frequently during gameplay.
• The beta DLL is meant for reporting bugs & issues with the client.

Hence use at your own risk.";
}

file sealed class UnpackagedInstall : MainDialog
{
    protected override string Title => "⚠️ Unpackaged Install";
    protected override string PrimaryButtonText => "Back";
    protected override string Content => @"The current Minecraft install is unpackaged.

• Please reinstall the game via the Microsoft or Xbox App.
• The launcher can only switch versions if the install is packaged.

If you need help, join our Discord.";
}
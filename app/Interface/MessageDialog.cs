using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ModernWpf.Controls;

namespace Flarial.Launcher.Interface;

abstract class MessageDialog
{
    internal MessageDialog() { }
    static readonly SemaphoreSlim s_semaphore = new(1, 1);

    internal async Task<bool> ShowAsync() => await PromptAsync() != ContentDialogResult.None;

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
    protected virtual string? CloseButtonText { get; }
    protected abstract string PrimaryButtonText { get; }
    protected virtual string? SecondaryButtonText { get; }

    internal static readonly MessageDialog _notInstalled = new NotInstalled();
    internal static readonly MessageDialog _launchFailure = new LaunchFailure();
    internal static readonly MessageDialog _selectVersion = new SelectVersion();
    internal static readonly MessageDialog _betaDllUsage = new BetaDllUsage();
    internal static readonly MessageDialog _folderNotFound = new FolderNotFound();
    internal static readonly MessageDialog _installVersion = new InstallVersion();
    internal static readonly MessageDialog _unsignedInstall = new UnsignedInstall();
    internal static readonly MessageDialog _invalidCustomDll = new InvalidCustomDll();
    internal static readonly MessageDialog _connectionFailure = new ConnectionFailure();
    internal static readonly MessageDialog _clientUpdateFailure = new ClientUpdateFailure();
    internal static readonly MessageDialog _unpackagedInstallation = new UnpackagedInstall();
    internal static readonly MessageDialog _gamingServicesMissing = new GamingServicesMissing();
    internal static readonly MessageDialog _launcherUpdateAvailable = new LauncherUpdateAvailable();

    sealed class GamingServicesMissing : MessageDialog
    {
        protected override string CloseButtonText => "Cancel";
        protected override string PrimaryButtonText => "Install";
        protected override string Title => "⚠️ Gaming Services Missing";
        protected override string Content => @"Gaming Services isn't installed, please install it.

• Gaming Services is required for installing GDK builds.
• You may install Gaming Services via the Microsoft Store.

If you need help, join our Discord.";
    }

    sealed class SelectVersion : MessageDialog
    {
        protected override string PrimaryButtonText => "Back";
        protected override string Title => "💡 Select Version";
        protected override string Content => @"No Minecraft version is selected.

• Select a Minecraft version from the list that should be installed.

If you need help, join our Discord.";
    }

    sealed class InstallVersion : MessageDialog
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

    sealed class FolderNotFound : MessageDialog
    {
        protected override string Title => "⚠️ Folder Not Found";

        protected override string Content => @"The client's folder cannot be found.

• Try launching the client at least once to generate its folder.

If you need help, join our Discord.";

        protected override string PrimaryButtonText => "Back";
    }

    sealed class NotInstalled : MessageDialog
    {
        protected override string CloseButtonText => "Cancel";
        protected override string PrimaryButtonText => "Install";
        protected override string Title => "⚠️ Not Installed";
        protected override string Content => @"Minecraft: Bedrock Edition isn't installed.

• Install Minecraft: Bedrock Edition via the Microsoft Store or Xbox App.

If you need help, join our Discord.";
    }

    sealed class ConnectionFailure : MessageDialog
    {
        protected override string PrimaryButtonText => "Exit";
        protected override string Title => "🚨 Connection Failure";
        protected override string Content => @"Failed to connect to Flarial Client Services.
        
• Try restarting the launcher.
• Check your internet connection.
• Change your system DNS for both IPv4 and IPv6.

If you need help, join our Discord.";
    }

    class UnsignedInstall : MessageDialog
    {
        protected override string Title => "⚠️ Unsigned Install";
        protected override string Content => @"An unsigned Minecraft install has been detected.

• The launcher will not wait for the game to initialize.
• Compatibility issues might arise with the client & launcher.

If you need help, join our Discord.";

        protected override string PrimaryButtonText => "Launch";
        protected override string? CloseButtonText => "Cancel";
    }

    sealed class InvalidCustomDll : MessageDialog
    {
        protected override string PrimaryButtonText => "Back";
        protected override string Title => "⚠️ Invalid Custom DLL";
        protected override string Content => @"The specified custom DLL is invalid.

• Specify a DLL that is valid and exists.
• If you didn't intend to use this feature, disable it.
• Ensure no security software is blocking the launcher.

If you need help, join our Discord.";
    }

    sealed class LaunchFailure : MessageDialog
    {
        protected override string Title => "⚠️ Launch Failure";
        protected override string PrimaryButtonText => "Back";
        protected override string Content => @"The launcher couldn't inject or initialize Minecraft correctly.

• Remove & disable any 3rd party mods or tools.
• Ensure no security software is blocking the launcher.
• Try closing Minecraft & launching it again via the launcher.

If you need help, join our Discord.";
    }

    sealed class ClientUpdateFailure : MessageDialog
    {
        protected override string PrimaryButtonText => "Back";
        protected override string Title => "⚠️ Client Update Failure";
        protected override string Content => @"A client update couldn't be downloaded.

• Try closing Minecraft & click on [Play] to update the client.
• Try rebooting your machine & see if that resolves the issue.

If you need help, join our Discord.";
    }

    sealed class LauncherUpdateAvailable : MessageDialog
    {
        protected override string Title => "💡 Launcher Update Available";
        protected override string PrimaryButtonText => "Update";
        protected override string CloseButtonText => "Later";
        protected override string Content => @"An update is available for the launcher.

• Updating the launcher provides fixes & new features.
• New versions of the client & Minecraft might require a launcher update.

If you need help, join our Discord.";
    }

    sealed class BetaDllUsage : MessageDialog
    {
        protected override string Title => "⚠️ Beta DLL Usage";
        protected override string CloseButtonText => "Cancel";
        protected override string PrimaryButtonText => "Launch";
        protected override string Content => @"The beta DLL of the client might be potentially unstable. 

• Bugs & crashes might occur frequently during gameplay.
• The beta DLL is meant for reporting bugs & issues with the client.

Hence use at your own risk.";
    }

    sealed class UnpackagedInstall : MessageDialog
    {
        protected override string Title => "⚠️ Unpackaged Install";
        protected override string PrimaryButtonText => "Back";
        protected override string Content => @"The current Minecraft install is unpackaged.

• Please reinstall the game via the Microsoft or Xbox App.
• The launcher can only switch versions if the install is packaged.

If you need help, join our Discord.";
    }
}
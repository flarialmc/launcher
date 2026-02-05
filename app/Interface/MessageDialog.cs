using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ModernWpf.Controls;

namespace Flarial.Launcher.Interface;

abstract class MessageDialog
{
    internal MessageDialog() { }

    static readonly ContentDialog s_dialog = new();
    static readonly SemaphoreSlim s_semaphore = new(1, 1);

    internal async Task<bool> ShowAsync() => await PromptAsync() != ContentDialogResult.None;

    internal async Task<ContentDialogResult> PromptAsync()
    {
        await s_semaphore.WaitAsync(); try
        {
            using (Application.Current.Dispatcher.DisableProcessing())
            {
                s_dialog.Title = Title;
                s_dialog.Content = Content;
                s_dialog.CloseButtonText = Close;
                s_dialog.PrimaryButtonText = Primary;
                s_dialog.SecondaryButtonText = Secondary;
            }
            return await s_dialog.ShowAsync(ContentDialogPlacement.InPlace);
        }
        finally { s_semaphore.Release(); }
    }

    protected abstract string Title { get; }
    protected abstract string Content { get; }
    protected abstract string Primary { get; }
    protected virtual string? Secondary { get; } = null;
    protected virtual string? Close { get; } = null;

    internal static readonly MessageDialog _connectionFailure = new ConnectionFailure();

    internal static readonly MessageDialog _notInstalled = new NotInstalled();

    internal static readonly MessageDialog _invalidCustomDll = new InvalidCustomDll();

    internal static readonly MessageDialog _launchFailure = new LaunchFailure();

    internal static readonly MessageDialog _clientUpdateFailure = new ClientUpdateFailure();

    internal static readonly MessageDialog _launcherUpdateAvailable = new LauncherUpdateAvailable();

    internal static readonly MessageDialog _betaDllEnabled = new BetaDllEnabled();

    internal static readonly MessageDialog _unpackagedInstallation = new UnpackagedInstall();

    internal static readonly MessageDialog _unsignedInstall = new UnsignedInstall();

    internal static readonly MessageDialog _folderNotFound = new FolderNotFound();

    internal static readonly MessageDialog _installVersion = new InstallVersion();

    internal static readonly MessageDialog _selectVersion = new SelectVersion();

    internal static readonly MessageDialog _allowUnsignedInstalls = new AllowUnsignedInstalls();

    internal static readonly MessageDialog _gamingServicesMissing = new GamingServicesMissing();

    sealed class GamingServicesMissing : MessageDialog
    {
        protected override string Close => "Back";
        protected override string Primary => "Install";
        protected override string Title => "⚠️ Gaming Services Missing";
        protected override string Content => @"Gaming Services isn't installed, please install it.

• Gaming Services is required for installing GDK builds.
• You may install Gaming Services via the Microsoft Store.

If you need help, join our Discord.";
    }

    sealed class SelectVersion : MessageDialog
    {
        protected override string Title => "💡 Select Version";

        protected override string Content => @"No Minecraft version is selected.

• Select a Minecraft version from the list that should be installed.

If you need help, join our Discord.";

        protected override string Primary => "Back";
    }

    sealed class InstallVersion : MessageDialog
    {
        protected override string Title => "💡 Install Version";

        protected override string Content => @"The selected Minecraft version will be now installed.
Once the installation starts, you won't able to cancel it.

• Free up disk space before proceeding with the installation.
• A high speed internet connection is recommended for this.

If you need help, join our Discord.";

        protected override string Primary => "Install";

        protected override string? Close => "Cancel";
    }

    sealed class FolderNotFound : MessageDialog
    {
        protected override string Title => "⚠️ Folder Not Found";

        protected override string Content => @"The client's folder cannot be found.

• Try launching the client at least once to generate its folder.

If you need help, join our Discord.";

        protected override string Primary => "Back";
    }

    sealed class NotInstalled : MessageDialog
    {
        protected override string Primary => "Back";
        protected override string Title => "⚠️ Not Installed";
        protected override string Content => @"Minecraft: Bedrock Edition isn't installed.

• Install Minecraft: Bedrock Edition via the Microsoft Store or Xbox App.

If you need help, join our Discord.";
    }

    sealed class ConnectionFailure : MessageDialog
    {
        protected override string? Close => "Exit";
        protected override string Primary => "Continue";
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

• Unsigned installs might cause compatibility issues with the client & launcher.

If you need help, join our Discord.";

        protected override string Primary => "Back";
    }

    sealed class AllowUnsignedInstalls : UnsignedInstall
    {
        protected override string? Close => "Launch";
    }

    sealed class InvalidCustomDll : MessageDialog
    {
        protected override string Primary => "Back";
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
        protected override string Primary => "Back";
        protected override string Content => @"The launcher couldn't inject or initialize Minecraft correctly.

• Remove & disable any 3rd party mods or tools.
• Ensure no security software is blocking the launcher.
• Try closing Minecraft & launching it again via the launcher.

If you need help, join our Discord.";
    }

    sealed class ClientUpdateFailure : MessageDialog
    {
        protected override string Primary => "Back";
        protected override string Title => "⚠️ Client Update Failure";
        protected override string Content => @"A client update couldn't be downloaded.

• Try closing Minecraft & click on [Play] to update the client.
• Try rebooting your machine & see if that resolves the issue.

If you need help, join our Discord.";
    }

    sealed class LauncherUpdateAvailable : MessageDialog
    {
        protected override string Title => "💡 Launcher Update Available";
        protected override string Primary => "Update";
        protected override string? Close => "Later";
        protected override string Content => @"An update is available for the launcher.

• Updating the launcher provides fixes & new features.
• New versions of the client & Minecraft might require a launcher update.

If you need help, join our Discord.";
    }

    sealed class BetaDllEnabled : MessageDialog
    {
        protected override string Title => "⚠️ Beta DLL Enabled";
        protected override string? Close => "Cancel";
        protected override string Primary => "Launch";
        protected override string Content => @"The beta DLL of the client might be potentially unstable. 

• Bugs & crashes might occur frequently during gameplay.
• The beta DLL is meant for reporting bugs & issues with the client.

Hence use at your own risk.";
    }

    sealed class UnpackagedInstall : MessageDialog
    {
        protected override string Title => "⚠️ Unpackaged Installation";
        protected override string Primary => "Back";
        protected override string Content => @"The current Minecraft installation is unpackaged.

• Please reinstall the game via the Microsoft or Xbox App.
• The launcher can only switch versions if the installation is packaged.

If you need help, join our Discord.";
    }
}
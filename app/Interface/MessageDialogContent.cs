using System;

namespace Flarial.Launcher.Interface;

abstract class MessageDialogContent
{
    public abstract string Title { get; }
    public abstract string Content { get; }
    public abstract string Primary { get; }
    public virtual string? Close { get; } = null;

    internal static readonly MessageDialogContent _connectionFailure = new ConnectionFailure();

    internal static readonly  MessageDialogContent _notInstalled = new NotInstalled();

    internal static readonly  MessageDialogContent _invalidCustomDll = new InvalidCustomDll();

    internal static readonly  MessageDialogContent _launchFailure = new LaunchFailure();

    internal static readonly  MessageDialogContent _clientUpdateFailure = new LauncherUpdateAvailable();

    internal static readonly  MessageDialogContent _launcherUpdateAvailable = new LauncherUpdateAvailable();

    internal static readonly  MessageDialogContent _betaDllEnabled = new BetaDllEnabled();

    internal static readonly  MessageDialogContent _unpackagedInstallationDetected = new UnpackagedInstallationDetected();

    internal static readonly  MessageDialogContent _unsignedInstallationDetected = new UnsignedInstallationDetected();

    sealed class NotInstalled : MessageDialogContent
    {
        public override string Primary => "Back";
        public override string Title => "⚠️ Not Installed";
        public override string Content => @"Minecraft: Bedrock Edition isn't installed.

• Install Minecraft: Bedrock Edition via the Microsoft Store or Xbox App.

If you need help, join our Discord.";
    }

    sealed class ConnectionFailure : MessageDialogContent
    {
        public override string? Close => "Exit";
        public override string Primary => "Continue";
        public override string Title => "🚨 Connection Failure";
        public override string Content => @"Failed to connect to Flarial's CDN.
        
• Try restarting the launcher.
• Check your internet connection.
• Change your system DNS for both IPv4 and IPv6.

If you need help, join our Discord.";
    }

    sealed class UnsignedInstallationDetected : MessageDialogContent
    {
        public override string Title => "⚠️ Unsigned Installation Detected";

        public override string Content => @"An unsigned Minecraft installation has been detected.

• Unsigned installs might cause compatibility issues with the client & launcher.
• Reinstall Minecraft via the Microsoft or Xbox App to fix this issue.

If you need help, join our Discord.";

        public override string Primary => "Back";
    }

    sealed class InvalidCustomDll : MessageDialogContent
    {
        public override string Primary => "Back";
        public override string Title => "⚠️ Invalid Custom DLL";
        public override string Content => @"The specified custom DLL is invalid.

• Specify a DLL that exists and valid.
• If you didn't intend to use a custom DLL, disable it in the launcher's settings.

If you need help, join our Discord.";
    }

    sealed class LaunchFailure : MessageDialogContent
    {
        public override string Title => "⚠️ Launch Failure";
        public override string Primary => "Back";
        public override string Content => @"The launcher couldn't inject or initialize Minecraft correctly.

• Remove & disable any 3rd party mods or tools.
• Try closing Minecraft & launching it again via the launcher.

If you need help, join our Discord.";
    }

    sealed class ClientUpdateFailure : MessageDialogContent
    {
        public override string Primary => "Back";
        public override string Title => "⚠️ Client Update Failure";
        public override string Content => @"A client update couldn't be downloaded.

• Try closing Minecraft & click on [Launch] to update the client.
• Try rebooting your machine & see if that resolves the issue.

If you need help, join our Discord.";
    }

    sealed class LauncherUpdateAvailable : MessageDialogContent
    {
        public override string Title => "💡 Launcher Update Available";
        public override string Primary => "Update";
        public override string? Close => "Later";
        public override string Content => @"An update is available for the launcher.

• Updating the launcher provides new bug fixes & features.
• Newer versions of the client & Minecraft might require a launcher update.

If you need help, join our Discord.";
    }

    sealed class BetaDllEnabled : MessageDialogContent
    {
        public override string Title => "⚠️ Beta DLL Enabled";
        public override string Primary => "Cancel";
        public override string? Close => "Launch";
        public override string Content => @"The beta DLL of the client might be potentially unstable. 

• Bugs & crashes might occur frequently during gameplay.
• The beta DLL is meant for reporting bugs & issues with the client.

Hence use at your own risk.";
    }

    sealed class UnpackagedInstallationDetected : MessageDialogContent
    {
        public override string Title => "⚠️ Unpackaged Installation Detected";
        public override string Primary => "Back";
        public override string Content => @"The current Minecraft installation is unpackaged.

• Please reinstall the game via the Microsoft or Xbox App.
• The launcher can only switch versions if the installation is packaged.

If you need help, join our Discord.";
    }
}
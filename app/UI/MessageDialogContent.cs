namespace Flarial.Launcher.UI;

abstract class MessageDialogContent
{
    public abstract string Title { get; }
    public abstract string Content { get; }
    public abstract string Primary { get; }
    public virtual string? Close { get; } = null;

    internal static readonly ConnectionFailure _connectionFailure = new();

    internal static readonly NotInstalled _notInstalled = new();

    internal static readonly UnsupportedVersion _unsupportedVersion = new();

    internal static readonly InvalidCustomDll _invalidCustomDll = new();

    internal static readonly LaunchFailure _launchFailure = new();

    internal static readonly UpdateFailure _updateFailure = new();

    internal static readonly LauncherUpdate _launcherUpdate = new();

    internal static readonly BetaUsage _betaUsage = new();

    internal static readonly NotSigned _notSigned = new();
}

sealed class NotInstalled : MessageDialogContent
{
    public override string Primary => "Back";
    public override string Title => "⚠️ Not Installed";
    public override string Content => @"Minecraft: Bedrock Edition isn't installed.

• Install the game via the Microsoft Store or Xbox App.
• Ensure the installed version is supported by Flarial.

If you need help, join our Discord.";
}

sealed class UnsupportedVersion : MessageDialogContent
{
    readonly string _currentVersion;
    readonly string _latestSupportedVersion;

    public UnsupportedVersion() : this("Unknown", "Unknown") { }

    public UnsupportedVersion(string currentVersion, string latestSupportedVersion)
    {
        _currentVersion = currentVersion;
        _latestSupportedVersion = latestSupportedVersion;
    }

    public override string Primary => "Back";
    public override string Title => "⚠️ Unsupported Version";
    public override string Content => $@"Your currently installed Minecraft version ({_currentVersion}) is not compatible with Flarial Client, please change your version to {_latestSupportedVersion} for the best experience.

• Install a game version that is supported by Flarial via the launcher.
• Try using the beta build of client by enabling in the launcher's settings.

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
    public override string Content => @"The launcher couldn't inject & initialize the game correctly.

• Try closing the game & try again.
• Remove & disable any 3rd party mods or tools.

If you need help, join our Discord.";
}

sealed class UpdateFailure : MessageDialogContent
{
    public override string Primary => "Back";
    public override string Title => "⚠️ Update Failure";
    public override string Content => @"A client update couldn't be downloaded.

• Try closing the game & see if the client updates.
• Try rebooting your machine & see if that resolves the issue.

If you need help, join our Discord.";
}

sealed class LauncherUpdate : MessageDialogContent
{
    public override string Title => "💡 Launcher Update";
    public override string Primary => "Update";
    public override string? Close => "Later";
    public override string Content => @"An update is available for the launcher.

• Updating the launcher provides new bug fixes & features.
• Newer versions of the client & game might require a launcher update.

If you need help, join our Discord.";
}

sealed class BetaUsage : MessageDialogContent
{
    public override string Title => "⚠️ Beta Usage";
    public override string Primary => "Cancel";
    public override string? Close => "Launch";
    public override string Content => @"The beta build of the client might be potentially unstable. 

• Bugs & crashes might occur frequently during gameplay.
• The beta build is meant for reporting bugs & issues with the client.

Hence use at your own risk.";
}

sealed class NotSigned : MessageDialogContent
{
    public override string Title => "⚠️ Not Signed";
    public override string Primary => "Back";
    public override string Content => @"The current game installation is unsigned.

• Reinstall the game via the Microsoft Store or Xbox App.
• Unsigned installations cannot be launched by the launcher.
• Unsigned installations cannot be updated or downgraded by the launcher.

If you need help, join our Discord.";
}
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
    public override string Primary => "Back";
    public override string Title => "⚠️ Unsupported Version";
    public override string Content => @"The currently installed game version isn't supported by Flarial.

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
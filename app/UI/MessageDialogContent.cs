namespace Flarial.Launcher.UI;

abstract class MessageDialogContent
{
    public abstract string Title { get; }
    public abstract string Content { get; }
    public abstract string Primary { get; }
    public virtual string? Close { get; } = null;

    internal static readonly ConnectionFailure _connectionFailure = new();

    internal static readonly VersionDownloading _versionDownloading = new();
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

sealed class VersionDownloading : MessageDialogContent
{
    public override string Title => "🚨 Version Downloading";
    public override string Primary => "Back";
    public override string Content => @"The launcher is downloading a game version.

• Wait for the download to finish.
• Once the download is finished, you may exit the launcher.

If you need help, join our Discord."; 
}


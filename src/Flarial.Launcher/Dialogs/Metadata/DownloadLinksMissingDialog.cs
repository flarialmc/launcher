using Flarial.Runtime.Versions;

namespace Flarial.Launcher.Dialogs.Metadata;

sealed class DownloadLinksMissingDialog(VersionItem version) : MessageDialog
{
    protected override string Title { get; } = "⚠️ Download Links Missing";

    protected override string Message { get; } = @$"Cannot find download links for Minecraft {version}.
        
• Try restarting the launcher.
• Check your internet connection.
• Change your system DNS for both IPv4 and IPv6.

If you need help, join our Discord.";

    protected override string Primary { get; } = "Back";
}

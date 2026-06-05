using Flarial.Runtime.Versions;

namespace Flarial.Launcher.Dialogs.Metadata;

sealed class InstallingVersionDialog(VersionItem version) : MessageDialog
{
    protected override string Title => "💡 Installing Version";
    protected override string Message => $@"Minecraft {version} is being installed by the launcher.

• Wait for the installation to finish.

If you need help, join our Discord.";

    protected override string Primary => "Back";
}
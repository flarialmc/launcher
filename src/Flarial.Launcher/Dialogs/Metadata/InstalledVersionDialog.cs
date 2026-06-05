using System;
using Flarial.Runtime.Versions;

namespace Flarial.Launcher.Dialogs.Metadata;

sealed class InstalledVersionDialog(VersionItem version) : MessageDialog
{
    protected override string Title => "💡 Installed Version";
    protected override string Message => $@"Minecraft {version} has been installed by the launcher.

• Verify if the correct game version has been installed.
• You may launch the game to verify the installed version.

If you need help, join our Discord.";

    protected override string Primary => "Back";
}
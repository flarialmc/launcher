using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flarial.Launcher.Dialogs.Metadata;

sealed class NotInstalledDialog : MessageDialog<NotInstalledDialog>
{
    protected override string Title { get; } = "⚠️ Not Installed";
    protected override string Message { get; } = @"Minecraft: Bedrock Edition isn't installed.

• Install Minecraft: Bedrock Edition via the Microsoft Store or Xbox App.

If you need help, join our Discord.";

    protected override string[] Buttons { get; } = ["Install", "Cancel"];
}
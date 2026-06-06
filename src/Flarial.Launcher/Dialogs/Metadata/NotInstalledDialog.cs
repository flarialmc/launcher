using System.Collections.Generic;
using System.Threading.Tasks;
using Flarial.Launcher.Management;

namespace Flarial.Launcher.Dialogs.Metadata;

sealed class NotInstalledDialog : MessageDialog<NotInstalledDialog>
{
    protected override string Title => "⚠️ Not Installed";
    protected override string Message => @"Minecraft: Bedrock Edition isn't installed.

• Install Minecraft: Bedrock Edition via the Microsoft Store or Xbox App.

If you need help, join our Discord.";

    protected override string Primary => "Install";

    protected override string Secondary => "Cancel";

    protected override async Task OnShowAsync(bool value) { if (value) MinecraftPage._.Open(); }
}
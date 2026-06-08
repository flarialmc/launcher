using System.Threading.Tasks;
using Flarial.Launcher.Interface.Presentation;
using Flarial.Launcher.Management;

namespace Flarial.Launcher.Interface.Dialogs;

sealed class NotInstalledDialog : AppDialog<NotInstalledDialog>
{
    protected override async Task OnShowAsync(bool value) { if (value) MinecraftPage._.Open(); }

    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Install";
    protected override string Title => "⚠️ Not Installed";
    protected override string Content => @"Minecraft: Bedrock Edition isn't installed.

• Install Minecraft: Bedrock Edition via the Microsoft Store or Xbox App.

If you need help, join our Discord.";
}

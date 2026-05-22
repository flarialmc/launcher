using System.Threading.Tasks;
using Flarial.Launcher.Interface.Presentation;
using Flarial.Launcher.Management;

namespace Flarial.Launcher.Interface.Dialogs;

sealed class NotInstalledDialog : AppDialog<NotInstalledDialog>
{
    internal override async Task<bool> OnShowAsync()
    {
        var _= await base.OnShowAsync();
        if (_) MinecraftPage.Open();
        return _;
    }

    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Install";
    protected override string Title => "⚠️ Not Installed";
    protected override string Content => @"Minecraft: Bedrock Edition isn't installed.

• Install Minecraft: Bedrock Edition via the Microsoft Store or Xbox App.

If you need help, join our Discord.";
}

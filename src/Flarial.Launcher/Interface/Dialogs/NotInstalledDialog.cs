using System.Threading.Tasks;
using Flarial.Launcher.Management;
using Windows.UI.Xaml;

namespace Flarial.Launcher.Interface.Dialogs;

sealed class NotInstalledDialog : MainDialog
{
    internal override async Task<bool> ShowAsync(UIElement element)
    {
        var result = await base.ShowAsync(element);
        if (result) MicrosoftStorePage.Minecraft.Open();
        return result;
    }

    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Install";
    protected override string Title => "⚠️ Not Installed";
    protected override string Content => @"Minecraft: Bedrock Edition isn't installed.

• Install Minecraft: Bedrock Edition via the Microsoft Store or Xbox App.

If you need help, join our Discord.";
}

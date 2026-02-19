using System.Threading.Tasks;
using Flarial.Launcher.Management;
using Windows.UI.Xaml;

namespace Flarial.Launcher.Interface.Dialogs;

sealed class GamingServicesMissingDialog : MainDialog
{
    internal override async Task<bool> ShowAsync(UIElement element)
    {
        var result = await base.ShowAsync(element);
        if (result) MicrosoftStorePage.GamingServices.Open();
        return result;
    }

    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Install";
    protected override string Title => "⚠️ Gaming Services Missing";
    protected override string Content => @"Gaming Services isn't installed, please install it.

• Gaming Services is required for GDK builds.
• You may install Gaming Services via the Microsoft Store.

If you need help, join our Discord.";
}

using System.Threading.Tasks;
using Flarial.Launcher.Interface.Presentation;
using Flarial.Launcher.Management;

namespace Flarial.Launcher.Interface.Dialogs;

sealed class GamingServicesMissingDialog : AppDialog<GamingServicesMissingDialog>
{
    protected override async Task OnShowAsync(bool value) { if (value) GamingServicesPage._.Open(); }

    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Install";
    protected override string Title => "⚠️ Gaming Services Missing";
    protected override string Content => @"Gaming Services isn't installed, please install it.

• Gaming Services is required for GDK builds.
• You may install Gaming Services via the Microsoft Store.

If you need help, join our Discord.";
}

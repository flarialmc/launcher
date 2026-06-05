using System.Threading.Tasks;
using Flarial.Launcher.Management;

namespace Flarial.Launcher.Dialogs.Metadata;

sealed class GamingServicesMissingDialog : MessageDialog<GamingServicesMissingDialog>
{
    protected override string Title => "⚠️ Gaming Services Missing";
    protected override string Message => @"Gaming Services isn't installed, please install it.

• Gaming Services is required for GDK builds.
• You may install Gaming Services via the Microsoft Store.

If you need help, join our Discord.";

    protected override string Primary => "Install";

    protected override string? Secondary => "Cancel";

    protected override async Task OnShowAsync(bool value) { if (value) GamingServicesPage.Open(); }
}

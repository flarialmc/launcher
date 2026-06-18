using System.Threading.Tasks;
using Flarial.Launcher.Management;

namespace Flarial.Launcher.Dialogs.Metadata;

sealed class GamingServicesMissingDialog : MessageDialog<GamingServicesMissingDialog>
{
    protected override string Title { get; } = "⚠️ Gaming Services Missing";
    protected override string Message { get; } = @"Gaming Services isn't installed, please install it.

• Gaming Services is required for GDK builds.
• You may install Gaming Services via the Microsoft Store.

If you need help, join our Discord.";

    protected override string Primary { get; } = "Install";

    protected override string? Secondary { get; } = "Cancel";

    protected override async Task OnShowAsync(bool value) { if (value) GamingServicesPage._.Open(); }
}

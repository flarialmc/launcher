using Flarial.Launcher.Interface.Presentation;

namespace Flarial.Launcher.Interface.Dialogs;

sealed class SideloadedInstallDialog : AppDialog<SideloadedInstallDialog>
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "⚠️ Sideloaded Install";
    protected override string Content => @"The game is currently sideloaded.

• Please reinstall the game via the Microsoft Store or Xbox App.
• The launcher can only switch versions if the game isn't sideloaded.

If you need help, join our Discord.";
}

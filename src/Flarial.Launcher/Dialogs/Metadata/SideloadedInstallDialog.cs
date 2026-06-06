namespace Flarial.Launcher.Dialogs.Metadata;

sealed class SideloadedInstallDialog : MessageDialog<SideloadedInstallDialog>
{
    protected override string Title => "⚠️ Sideloaded Install";

    protected override string Message => @"The game is currently sideloaded.

• Please reinstall the game via the Microsoft Store or Xbox App.
• The launcher can only switch versions if the game isn't sideloaded.

If you need help, join our Discord.";

    protected override string Primary => "Back";
}

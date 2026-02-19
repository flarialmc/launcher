namespace Flarial.Launcher.Interface.Dialogs;

sealed class AllowUnsignedInstallDialog : UnsignedInstallDialog
{
    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Launch";
}

class UnsignedInstallDialog : MainDialog
{
    protected override string Title => "⚠️ Unsigned Install";
    protected override string PrimaryButtonText => "Back";
    protected override string Content => @"The launcher cannot verify the integrity of the game.

• Compatibility issues might arise with the client & launcher.
• Consider reinstalling the game via the Microsoft Store or Xbox App.

If you need help, join our Discord.";
}

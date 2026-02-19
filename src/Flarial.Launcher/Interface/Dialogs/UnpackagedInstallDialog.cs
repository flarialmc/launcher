namespace Flarial.Launcher.Interface.Dialogs;

sealed class UnpackagedInstallDialog : MainDialog
{
    protected override string Title => "⚠️ Unpackaged Install";
    protected override string PrimaryButtonText => "Back";
    protected override string Content => @"The current Minecraft install is unpackaged.

• Please reinstall the game via the Microsoft or Xbox App.
• The launcher can only switch versions if the install is packaged.

If you need help, join our Discord.";
}

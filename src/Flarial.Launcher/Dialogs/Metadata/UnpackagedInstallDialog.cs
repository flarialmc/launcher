namespace Flarial.Launcher.Dialogs.Metadata;

sealed class UnpackagedInstallDialog : MessageDialog<UnpackagedInstallDialog>
{
    protected override string Title => "⚠️ Unpackaged Install";

    protected override string Message => @"The current Minecraft install is unpackaged.

• Please reinstall the game via the Microsoft or Xbox App.
• The launcher can only switch versions if the install isn't sideloaded.

If you need help, join our Discord.";

    protected override string Primary => "Back";
}

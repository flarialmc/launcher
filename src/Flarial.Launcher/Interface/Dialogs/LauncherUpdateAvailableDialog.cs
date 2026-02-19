namespace Flarial.Launcher.Interface.Dialogs;

sealed class LauncherUpdateAvailableDialog : MainDialog
{
    protected override string Title => "💡 Launcher Update Available";
    protected override string PrimaryButtonText => "Update";
    protected override string CloseButtonText => "Later";
    protected override string Content => @"An update is available for the launcher.

• Updating the launcher provides fixes & new features.
• New versions of the client & Minecraft might require a launcher update.

If you need help, join our Discord.";
}

namespace Flarial.Launcher.Dialogs.Metadata;

sealed class LauncherUpdateAvailableDialog : MessageDialog<LauncherUpdateAvailableDialog>
{
    protected override string Title => "💡 Launcher Update Available";

    protected override string Message => @"An update is available for the launcher.

• Updating the launcher provides fixes & new features.
• An update might be required for newer game & client versions.

If you need help, join our Discord.";

    protected override string Primary => "Update";

    protected override string Secondary => "Later";
}

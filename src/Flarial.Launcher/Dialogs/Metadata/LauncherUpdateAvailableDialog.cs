namespace Flarial.Launcher.Dialogs.Metadata;

sealed class LauncherUpdateAvailableDialog : MessageDialog<LauncherUpdateAvailableDialog>
{
    protected override string Title { get; } = "💡 Launcher Update Available";

    protected override string Message { get; } = @"An update is available for the launcher.

• Updating the launcher provides fixes & new features.
• An update might be required for newer game & client versions.

If you need help, join our Discord.";

    protected override string Primary { get; } = "Update";

    protected override string Secondary { get; } = "Later";
}

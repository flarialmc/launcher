namespace Flarial.Launcher.Dialogs.Metadata;

sealed class LauncherMigrationDialog : MessageDialog<LauncherMigrationDialog>
{
    protected override string Title => "💡 Launcher Migration";

    protected override string Message => @"To continue using the launcher, you must migrate to MSIX.

• Administrator permissions are required to proceed with this.
• Clicking [Update] will start the launcher's migration process.

If you need help, join our Discord.";

    protected override string Primary => "Update";
    protected override string Secondary => "Exit";
}

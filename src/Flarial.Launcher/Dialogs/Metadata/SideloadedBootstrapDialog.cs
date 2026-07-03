namespace Flarial.Launcher.Dialogs.Metadata;

sealed class SideloadedBootstrapDialog : MessageDialog<SideloadedBootstrapDialog>
{
    protected override string Title { get; } = "⚠️ Sideloaded Bootstrap";

    protected override string Message { get; } = @"The launcher cannot safely bootstrap sideloaded game installs.

• Launch the game manually and wait until it initializes.
• Click on [Continue] to launch the game at your own risk.

If you need help, join our Discord.";

    protected override string Primary { get; } = "Continue";

    protected override string Secondary { get; } = "Back";
}

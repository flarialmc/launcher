namespace Flarial.Launcher.Dialogs.Metadata;

sealed class CompatibilityModeDialog : MessageDialog<CompatibilityModeDialog>
{
    protected override string Title => "⚠️ Compatibility Mode";

    protected override string Message => @"The launcher is currently in compatibility mode.

• Consider launching the game manually & then pressing [Launch].
• The game is currently sideloaded hence cannot be launched safely.

If you need help, join our Discord.";

    protected override string Primary => "Back";
}

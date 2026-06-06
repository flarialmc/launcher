namespace Flarial.Launcher.Dialogs.Metadata;

sealed class CompatibilityModeDialog : MessageDialog<CompatibilityModeDialog>
{
    protected override string Title => "⚠️ Compatibility Mode";

    protected override string Message => @"The launcher is currently in compatibility mode.

• The game is currently unpackaged, cannot launch safely.
• Instead, launch the game manually & then press [Launch].

If you need help, join our Discord.";

    protected override string Primary => "Back";
}

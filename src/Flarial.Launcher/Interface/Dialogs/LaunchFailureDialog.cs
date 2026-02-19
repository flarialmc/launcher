namespace Flarial.Launcher.Interface.Dialogs;

sealed class LaunchFailureDialog : MainDialog
{
    protected override string Title => "⚠️ Launch Failure";
    protected override string PrimaryButtonText => "Back";
    protected override string Content => @"The launcher couldn't inject or initialize Minecraft correctly.

• Remove & disable any 3rd party mods or tools.
• Ensure no security software is blocking the launcher.
• Try closing Minecraft & launching it again via the launcher.

If you need help, join our Discord.";
}

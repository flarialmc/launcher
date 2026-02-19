namespace Flarial.Launcher.Interface.Dialogs;

sealed class UWPDeprecatedDialog : MainDialog
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "⚠️ UWP Deprecated";
    protected override string Content => @"The client & launcher no longer support UWP builds.

• UWP builds are now outdated & deprecated.
• Consider updating to the latest GDK build of the game.

If you need help, join our Discord.";
}

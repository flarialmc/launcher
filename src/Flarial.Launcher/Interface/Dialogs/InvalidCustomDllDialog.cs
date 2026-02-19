namespace Flarial.Launcher.Interface.Dialogs;

sealed class InvalidCustomDllDialog : MainDialog
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "⚠️ Invalid Custom DLL";
    protected override string Content => @"The specified custom DLL is invalid.

• Specify a DLL that is valid and exists.
• If you didn't intend to use this feature, disable it.
• Ensure no security software is blocking the launcher.

If you need help, join our Discord.";
}

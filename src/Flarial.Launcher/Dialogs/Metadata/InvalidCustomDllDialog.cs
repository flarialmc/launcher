namespace Flarial.Launcher.Dialogs.Metadata;

sealed class InvalidCustomDllDialog : MessageDialog<InvalidCustomDllDialog>
{
    protected override string Title => "⚠️ Invalid Custom DLL";

    protected override string Message => @"The specified custom DLL is invalid.

• Specify a DLL that is valid and exists.
• If you didn't intend to use this feature, disable it.
• Ensure no security software is blocking the launcher.

If you need help, join our Discord.";

    protected override string Primary => "Back";
}

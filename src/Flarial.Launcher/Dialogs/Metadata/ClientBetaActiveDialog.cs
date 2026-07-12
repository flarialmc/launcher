namespace Flarial.Launcher.Dialogs.Metadata;

sealed class ClientBetaActiveDialog : MessageDialog<ClientBetaActiveDialog>
{
    protected override string Title => "⚠️ Client Beta Active";

    protected override string Message => @"The client's beta has been selected.

• These beta builds might be unstable and buggy.
• The builds are only for feedback & testing purposes only.

If you need help, join our Discord.";

    protected override string Primary => "Launch";
    protected override string Secondary => "Back";
}

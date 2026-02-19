namespace Flarial.Launcher.Interface.Dialogs;

sealed class ClientUpdateFailureDialog : MainDialog
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "⚠️ Client Update Failure";
    protected override string Content => @"A client update couldn't be downloaded.

• Try closing Minecraft & click on [Play] to update the client.
• Try rebooting your machine & see if that resolves the issue.

If you need help, join our Discord.";
}

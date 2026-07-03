namespace Flarial.Launcher.Dialogs.Metadata;

sealed class ClientUpdateFailureDialog : MessageDialog<ClientUpdateFailureDialog>
{
    protected override string Title { get; } = "⚠️ Client Update Failure";

    protected override string Message { get; } = @"A client update couldn't be downloaded.

• Try closing Minecraft & click on [Play] to update the client.
• Try rebooting your machine & see if that resolves the issue.

If you need help, join our Discord.";

    protected override string Primary { get; } = "Back";
}

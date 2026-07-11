namespace Flarial.Launcher.Dialogs.Metadata;

sealed class ClientAlreadyInjectedDialog : MessageDialog<ClientAlreadyInjectedDialog>
{
    protected override string Title => "⚠️ Client Already Injected";

    protected override string Message => @"The client is already injected into the game.

• Switch to the game manually to use the client.
• If this wasn't intended, please close the game & try again.

If you need help, join our Discord.";

    protected override string Primary => "Back";
}

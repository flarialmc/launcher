namespace Flarial.Launcher.Interface.Dialogs;

sealed class ClientInjectionFailureDialog : MainDialog
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "⚠️ Client Injection Failure";
    protected override string Content => @"The launcher couldn't inject the client into the game.
    
• Ensure no security software is blocking the launcher & client.
• Add the launcher & client as exclusions in installed security software.

If you need help, join our Discord.";
}
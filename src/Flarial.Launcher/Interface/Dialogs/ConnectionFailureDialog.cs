namespace Flarial.Launcher.Interface.Dialogs;

sealed class ConnectionFailureDialog : MasterDialog
{
    protected override string PrimaryButtonText => "Exit";
    protected override string Title => "🚨 Connection Failure";
    protected override string Content => @"Failed to connect to Flarial Client Services.
        
• Try restarting the launcher.
• Check your internet connection.
• Change your system DNS for both IPv4 and IPv6.

If you need help, join our Discord.";
}

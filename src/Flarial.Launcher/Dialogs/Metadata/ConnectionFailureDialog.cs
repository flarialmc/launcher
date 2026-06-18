namespace Flarial.Launcher.Dialogs.Metadata;

sealed class ConnectionFailureDialog : MessageDialog<ConnectionFailureDialog>
{
    protected override string Title { get; } = "🚨 Connection Failure";

    protected override string Message { get; } = @"Failed to connect to Flarial Client Services.
        
• Try restarting the launcher.
• Check your internet connection.
• Change your system DNS for both IPv4 and IPv6.

If you need help, join our Discord.";

    protected override string Primary { get; } = "Exit";
}

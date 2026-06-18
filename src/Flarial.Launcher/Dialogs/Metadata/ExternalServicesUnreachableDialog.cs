namespace Flarial.Launcher.Dialogs.Metadata;

sealed class ExternalServicesUnreachableDialog : MessageDialog<ExternalServicesUnreachableDialog>
{
    protected override string Title { get; } = "🚨 External Services Unreachable";

    protected override string Message { get; } = @"Failed to connect to External Services.
        
• Try restarting the launcher.
• Check your internet connection.
• Change your system DNS for both IPv4 and IPv6.

If you need help, join our Discord.";

    protected override string Primary { get; } = "Exit";
}

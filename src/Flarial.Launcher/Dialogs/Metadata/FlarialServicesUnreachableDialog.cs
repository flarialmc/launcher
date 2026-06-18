namespace Flarial.Launcher.Dialogs.Metadata;

sealed class FlarialServicesUnreachableDialog : MessageDialog<FlarialServicesUnreachableDialog>
{
    protected override string Title { get; } = "🚨 Flarial Services Unavaiable";

    protected override string Message { get; } = @"Failed to connect to Flarial Services.
        
• Try restarting the launcher.
• Check your internet connection.
• Change your system DNS for both IPv4 and IPv6.

If you need help, join our Discord.";

    protected override string Primary { get; } = "Exit";
}

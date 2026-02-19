namespace Flarial.Launcher.Interface.Dialogs;

sealed class BetaDllUsageDialog : MainDialog
{
    protected override string Title => "⚠️ Beta DLL Usage";
    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Launch";
    protected override string Content => @"The beta DLL of the client might be potentially unstable. 

• Bugs & crashes might occur frequently during gameplay.
• The beta DLL is meant for reporting bugs & issues with the client.

Hence use at your own risk.";
}

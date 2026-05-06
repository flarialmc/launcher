namespace Flarial.Launcher.Interface.Dialogs;

sealed class LogExportFailureDialog : MasterDialog
{
    readonly string _reason;

    internal LogExportFailureDialog(string reason) => _reason = reason;

    protected override string Title => "Export Failed";
    protected override string PrimaryButtonText => "OK";
    protected override string Content => $"Failed to export logs:\n\n{_reason}";
}

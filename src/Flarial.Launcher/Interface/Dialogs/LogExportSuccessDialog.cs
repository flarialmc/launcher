namespace Flarial.Launcher.Interface.Dialogs;

sealed class LogExportSuccessDialog : MasterDialog
{
    readonly string _path;

    internal LogExportSuccessDialog(string path) => _path = path;

    protected override string Title => "Logs Exported";
    protected override string PrimaryButtonText => "OK";
    protected override string Content => $"Logs and crash files have been exported to:\n\n{_path}";
}

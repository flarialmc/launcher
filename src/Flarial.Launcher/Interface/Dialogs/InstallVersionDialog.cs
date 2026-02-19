namespace Flarial.Launcher.Interface.Dialogs;

sealed class InstallVersionDialog : MainDialog
{
    protected override string Title => "💡 Install Version";
    protected override string PrimaryButtonText => "Install";
    protected override string CloseButtonText => "Cancel";

    protected override string Content => @"The selected Minecraft version will be now installed.
Once the installation starts, you won't able to cancel it.

• Free up disk space before proceeding with the installation.
• A high speed internet connection is recommended for this.

If you need help, join our Discord.";
}

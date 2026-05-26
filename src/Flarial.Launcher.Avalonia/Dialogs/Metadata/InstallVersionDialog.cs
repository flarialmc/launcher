namespace Flarial.Launcher.Dialogs.Metadata;

sealed class InstallVersionDialog(string version) : MessageDialog
{
    protected override string Title { get; } = "💡 Install Version";
    protected override string[] Buttons { get; } = ["Install", "Cancel"];
    protected override string Message { get; } = @$"Minecraft {version} will be now installed.
Once the installation starts, you won't able to cancel it.

• Free up disk space before proceeding with the installation.
• A high speed internet connection is recommended for this.

If you need help, join our Discord.";
}

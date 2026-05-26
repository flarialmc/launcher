namespace Flarial.Launcher.Dialogs.Metadata;

sealed class VersionInstallingDialog(string version) : MessageDialog
{
    protected override string Title { get; } = "💡 Version Installing";
    protected override string Message { get; } = $@"Minecraft {version} is being installed by the launcher.

• Wait for the installation to finish.

If you need help, join our Discord.";
    protected override string[] Buttons { get; } = ["Back"];
}
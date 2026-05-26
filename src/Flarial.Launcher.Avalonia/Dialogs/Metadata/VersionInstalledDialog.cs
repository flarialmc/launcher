namespace Flarial.Launcher.Dialogs.Metadata;

sealed class VersionInstalledDialog(string version) : MessageDialog
{
    protected override string Title { get; } = "💡 Version Installed";
    protected override string Message { get; } = $@"Minecraft {version} has been installed by the launcher.

• Verify if the correct game version has been installed.
• You may launch the game to verify the installed version.

If you need help, join our Discord.";
    protected override string[] Buttons { get; } = ["Back"];
}
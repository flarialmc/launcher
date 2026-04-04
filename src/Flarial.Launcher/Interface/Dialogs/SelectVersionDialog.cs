namespace Flarial.Launcher.Interface.Dialogs;

sealed class SelectVersionDialog : MasterDialog
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "💡 Select Version";
    protected override string Content => @"No Minecraft version is selected.

• Select a Minecraft version from the list that should be installed.

If you need help, join our Discord.";
}

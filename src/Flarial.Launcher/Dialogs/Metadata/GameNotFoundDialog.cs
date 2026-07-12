namespace Flarial.Launcher.Dialogs.Metadata;

sealed class GameNotFoundDialog : MessageDialog<GameNotFoundDialog>
{
    protected override string Title => "⚠️ Game Not Found";

    protected override string Message => @$"The launcher cannot find an instance of Minecraft.

• Make sure there is a running instance of the game available.
• Try installing the game via the Microsoft Store or Xbox App.

If you need help, join our Discord.";

    protected override string Primary => "Back";
}

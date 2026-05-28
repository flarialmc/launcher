using System.Threading.Tasks;

namespace Flarial.Launcher.Dialogs.Metadata;

public sealed class BetaBuildDialog : MessageDialog<BetaBuildDialog>
{
    protected override string Title { get; } = "⚠️ Beta Build";
    protected override string Message { get; } = @"This is a beta build of the launcher.

• This build doesn't represent the final iteration of the launcher.
• Please provide feedback and bug reports for this launcher build.

If you need help, join our Discord!";
    protected override string[] Buttons { get; } = ["Continue"];

    internal override async Task<bool> OnShowAsync()
    {
        await base.OnShowAsync();
        return false;
    }
}
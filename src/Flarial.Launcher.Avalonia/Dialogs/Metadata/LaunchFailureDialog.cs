using System.Collections.Generic;

namespace Flarial.Launcher.Dialogs.Metadata;

sealed class LaunchFailureDialog : MessageDialog
{
    protected override string Title { get; } = "⚠️ Launch Failure";
    protected override string[] Buttons { get; } = ["Back"];
    protected override string Message { get; } = @"The launcher couldn't inject or initialize Minecraft correctly.

• Remove & disable any 3rd party mods or tools.
• Ensure no security software is blocking the launcher.
• Try closing Minecraft & launching it again via the launcher.

If you need help, join our Discord.";
}

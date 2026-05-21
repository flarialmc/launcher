using System.Collections.Generic;

namespace Flarial.Launcher.Dialogs.Metadata;

sealed class ExampleDialog : MessageDialog
{
    protected override string Title { get; } = "Example";
    protected override string Message { get; } = @"This is an example!

• This is the first point.
• This is the second point.
• This is the third point.

Ask for help!";
    protected override IReadOnlyList<string> Buttons { get; } = ["OK", "Cancel"];
}
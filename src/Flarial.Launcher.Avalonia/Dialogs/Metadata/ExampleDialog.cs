using System.Collections.Generic;

namespace Flarial.Launcher.Dialogs.Metadata;

public sealed class ExampleDialog : MessageDialog<ExampleDialog>
{
    protected override string Title { get; } = "Example";
    protected override string Message { get; } = @"This is an example!

• This is the first point.
• This is the second point.
• This is the third point.

Ask for help!";
    protected override string[] Buttons { get; } = ["Ok", "Cancel", "Yes", "No"];
}
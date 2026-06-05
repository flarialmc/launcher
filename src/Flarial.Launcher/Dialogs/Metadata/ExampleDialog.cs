using System;

namespace Flarial.Launcher.Dialogs.Metadata;

public sealed class ExampleDialog : MessageDialog<ExampleDialog>
{
    protected override string Title => "Example";
   
    protected override string Message => @"This is an example!

• This is the first point.
• This is the second point.
• This is the third point.

Ask for help!";

    protected override string Primary => "OK";

    protected override string Secondary => "Back";
}
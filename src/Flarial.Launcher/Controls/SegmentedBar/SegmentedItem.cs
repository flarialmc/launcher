using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.Controls.SegmentedBar;

public sealed partial class SegmentItem : ReactiveObject
{
    [Reactive] object? _tag = null;
    [Reactive] string? _tooltip = null;
    [Reactive] bool _isEnabled = true;
    [Reactive] string _title = string.Empty;
}
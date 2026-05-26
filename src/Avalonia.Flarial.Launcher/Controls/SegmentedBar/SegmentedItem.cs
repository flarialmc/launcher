namespace Flarial.Launcher.Controls.SegmentedBar;

public class SegmentItem
{
    public string Title { get; set; } = "";
    public bool IsEnabled { get; set; } = true;
    public object? Tag { get; set; }
    public string? Tooltip { get; set; } = "";
}
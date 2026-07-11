using Avalonia.Media;

namespace Flarial.Launcher.Models;

public class Role
{
    public string Name { get; set; } = string.Empty;
    public Brush BackgroundBrush { get; set; } = SolidColorBrush.Parse("#00ffffff");
    public Brush BorderBrush { get; set; } = SolidColorBrush.Parse("#00ffffff");
}
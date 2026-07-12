using Avalonia.Media;

namespace Flarial.Launcher.Models;

public sealed class DiscordRoleModel
{
    public string RoleName { get; set; } = string.Empty;
    public Brush BorderBrush { get; set; } = SolidColorBrush.Parse("#00ffffff");
    public Brush BackgroundBrush { get; set; } = SolidColorBrush.Parse("#00ffffff");
}
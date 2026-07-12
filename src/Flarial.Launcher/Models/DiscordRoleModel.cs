using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.Models;

public sealed partial class DiscordRoleModel : ReactiveObject
{
    [Reactive] string _name = string.Empty;
    [Reactive] IBrush _border = Brushes.Transparent;
    [Reactive] IBrush _background = Brushes.Transparent;
}
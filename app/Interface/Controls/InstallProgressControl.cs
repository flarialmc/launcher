using ModernWpf.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Flarial.Launcher.UI.Controls;

sealed class InstallProgressControl : Grid
{
    internal readonly Button _button = new()
    {
        Content = "Install",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
    };

    internal readonly ModernWpf.Controls.ProgressBar _progressBar = new()
    {
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Foreground = new SolidColorBrush(Colors.White),
        Visibility = Visibility.Hidden
    };

    internal readonly SymbolIcon _icon = new()
    {
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Left,
        Margin = new(12, 0, 12, 0),
        Visibility = Visibility.Collapsed
    };

    internal InstallProgressControl()
    {
        ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        ColumnDefinitions.Add(new());

        SetRow(_icon, 0);
        SetColumn(_icon, 0);
        Children.Add(_icon);

        SetRow(_button, 0);
        SetColumn(_button, 1);
        Children.Add(_button);

        SetRow(_progressBar, 0);
        SetColumn(_progressBar, 1);
        Children.Add(_progressBar);
    }
}
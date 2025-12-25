using ModernWpf.Controls;
using Modern = ModernWpf.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Flarial.Launcher.UI.Controls;

sealed class InstallProgressControl : Grid
{
    internal readonly Button _button = new()
    {
        Content = new SymbolIcon(Symbol.Download),
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
    };

    internal readonly Modern.ProgressBar _progressBar = new()
    {
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Foreground = new SolidColorBrush(Colors.White),
        Visibility = Visibility.Hidden
    };

    internal InstallProgressControl()
    {
        ColumnDefinitions.Add(new());
        ColumnDefinitions.Add(new() { Width = GridLength.Auto });

        SetRow(_button, 0);
        SetColumn(_button, 0);
        Children.Add(_button);

        SetRow(_progressBar, 0);
        SetColumn(_progressBar, 0);
        Children.Add(_progressBar);
    }
}
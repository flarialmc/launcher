using System;
using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls;

namespace Flarial.Launcher.Interface.Controls;

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
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Visibility = Visibility.Collapsed
    };

    internal readonly TextBlock _textBlock = new()
    {
        Text = "Preparing...",
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Visibility = Visibility.Collapsed
    };

    [Obsolete("", true)]
    internal readonly Grid _grid = new()
    {
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Visibility = Visibility.Collapsed
    };

    [Obsolete("", true)]
    internal readonly SymbolIcon _icon = new()
    {
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Left,
        Margin = new(12, 0, 12, 0),
        Visibility = Visibility.Collapsed
    };

    internal InstallProgressControl()
    {
        SetRow(_button, 0);
        SetColumn(_button, 0);
        Children.Add(_button);

        SetRow(_progressBar, 0);
        SetColumn(_progressBar, 0);
        Children.Add(_progressBar);

        SetRow(_textBlock, 0);
        SetColumn(_textBlock, 0);
        Children.Add(_textBlock);

        SetZIndex(_textBlock, int.MaxValue);
        SetZIndex(_progressBar, int.MinValue);
    }
}
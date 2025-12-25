using System.Windows;
using System.Windows.Media;
using Flarial.Launcher.App;
using ModernWpf.Controls;
using System.Windows.Controls;
using Flarial.Launcher.Services.SDK;
using Flarial.Launcher.Services.Management.Versions;

namespace Flarial.Launcher.UI.Pages;

sealed class HomePage : Grid
{
    readonly VersionCatalog _catalog;

    readonly Image _image = new()
    {
        Source = Manifest.Icon,
        Width = Manifest.Icon.Width / 2,
        Height = Manifest.Icon.Height / 2,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new(0, 0, 0, 120)
    };

    readonly ModernWpf.Controls.ProgressBar _progressBar = new()
    {
        Width = Manifest.Icon.Width * 2,
        Foreground = new SolidColorBrush(Colors.White),
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        IsIndeterminate = true,
        Margin = new(0, 120, 0, 0),
        Visibility = Visibility.Collapsed
    };

    readonly TextBlock _textBlock = new()
    {
        Text = "Preparing...",
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new(0, 60, 0, 0),
        Visibility = Visibility.Collapsed
    };

    readonly Button _button = new()
    {
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Content = new SymbolIcon(Symbol.Play),
        Width = Manifest.Icon.Width * 2,
        Margin = new(0, 120, 0, 0)
    };

    internal HomePage(VersionCatalog catalog)
    {
        Children.Add(_image);
        Children.Add(_progressBar);
        Children.Add(_textBlock);
        Children.Add(_button);
        Children.Add(new TextBlock
        {
            Text = Manifest.Version,
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new(0, 0, 12, 12)
        });

        _catalog = catalog;

        _button.Click += (_, _) =>
        {

        };
    }
}
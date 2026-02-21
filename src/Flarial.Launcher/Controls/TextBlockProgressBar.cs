using Flarial.Launcher.Xaml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Controls;

sealed class TextBlockProgressBar : Grid
{
    internal readonly ProgressBar _progressBar = new()
    {
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Visibility = Visibility.Collapsed
    };

    internal readonly TextBlock _textBlock = new()
    {
        Text = "Downloading...",
        Margin = new(0, 0, 0, 1),
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Visibility = Visibility.Collapsed
    };

    internal TextBlockProgressBar()
    {
        Children.Add(_progressBar);
        Children.Add(_textBlock);
    }
}
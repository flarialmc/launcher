using Flarial.Launcher.Xaml;
using Flarial.Runtime.Services;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static Windows.Win32.Foundation.HWND;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;

namespace Flarial.Launcher.Controls;

sealed class PromotionImage : XamlElement<Border>
{
    readonly CoreWindow _window = CoreWindow.GetForCurrentThread();

    static readonly CoreCursor _hand = new(CoreCursorType.Hand, 0);
    static readonly CoreCursor _arrow = new(CoreCursorType.Arrow, 0);

    internal PromotionImage(Promotion promotion) : base(new())
    {
        (~this).Width = 320 * 0.95;
        (~this).Height = 50 * 0.95;
        (~this).Visibility = Visibility.Collapsed;

        (~this).BorderThickness = new(1);
        (~this).BorderBrush = new SolidColorBrush(Colors.IndianRed);

        BitmapImage image = new()
        {
            UriSource = new(promotion.Image),
            DecodePixelWidth = (int)(~this).Width,
            DecodePixelHeight = (int)(~this).Height,
            DecodePixelType = DecodePixelType.Logical
        };

        (~this).Tag = promotion.Uri;
        (~this).Background = new ImageBrush { ImageSource = image };

        image.ImageOpened += OnImageOpened;
        (~this).PointerExited += OnPointerExited;
        (~this).PointerEntered += OnPointerEntered;
        (~this).PointerPressed += OnPointerPressed;
    }

    void OnPointerEntered(object sender, RoutedEventArgs args)
    {
        _window.PointerCursor = _hand;
    }

    void OnPointerExited(object sender, RoutedEventArgs args)
    {
        _window.PointerCursor = _arrow;
    }

    unsafe void OnPointerPressed(object sender, RoutedEventArgs args)
    {
        fixed (char* lpFile = (string)(~this).Tag)
            ShellExecute(Null, null, lpFile, null, null, SW_NORMAL);
    }

    void OnImageOpened(object sender, RoutedEventArgs args)
    {
        (~this).Visibility = Visibility.Visible;
    }
}
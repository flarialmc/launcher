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
    static readonly CoreCursor s_hand = new(CoreCursorType.Hand, 0);
    static readonly CoreCursor s_arrow = new(CoreCursorType.Arrow, 0);

    internal PromotionImage(Promotion promotion) : base(new())
    {
        Image image = new()
        {
            Width = 320 * 0.95,
            Height = 50 * 0.95,
            Tag = promotion.Uri,
            Source = new BitmapImage
            {
                UriSource = new(promotion.Image),
                DecodePixelWidth = (int)(320 * 0.95),
                DecodePixelHeight = (int)(50 * 0.95),
                DecodePixelType = DecodePixelType.Logical,
            }
        };

        image.ImageOpened += OnImageOpened;
        image.PointerExited += OnPointerExited;
        image.PointerEntered += OnPointerEntered;
        image.PointerPressed += OnPointerPressed;

        (~this).Child = image;
        (~this).BorderThickness = new(1);
        (~this).Visibility = Visibility.Collapsed;
        (~this).BorderBrush = new SolidColorBrush(Colors.IndianRed);
    }

    void OnImageOpened(object sender, RoutedEventArgs args) => (~this).Visibility = Visibility.Visible;

    unsafe static void OnPointerPressed(object sender, RoutedEventArgs args)
    {
        fixed (char* lpFile = (string)((Image)sender).Tag)
            ShellExecute(Null, null, lpFile, null, null, SW_NORMAL);
    }

    static void OnPointerEntered(object sender, RoutedEventArgs args)
    {
        var window = CoreWindow.GetForCurrentThread();
        window.PointerCursor = s_hand;
    }

    static void OnPointerExited(object sender, RoutedEventArgs args)
    {
        var window = CoreWindow.GetForCurrentThread();
        window.PointerCursor = s_arrow;
    }
}
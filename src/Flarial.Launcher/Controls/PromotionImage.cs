using System;
using Flarial.Runtime.Services;
using Flarial.Launcher.Xaml;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using static Windows.Win32.Foundation.HWND;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;

namespace Flarial.Launcher.Controls;

sealed class PromotionImage : XamlElement<Image>
{
    static readonly CoreCursor _hand = new(CoreCursorType.Hand, 0);
    static readonly CoreCursor _arrow = new(CoreCursorType.Arrow, 0);

    internal PromotionImage(Promotion promotion) : base(new())
    {
        (~this).Width = 320 * 0.95;
        (~this).Height = 50 * 0.95;

        (~this).Tag = promotion.Uri;
        (~this).Visibility = Visibility.Collapsed;

        (~this).Source = new BitmapImage
        {
            UriSource = new(promotion.Image),
            DecodePixelWidth = (int)(~this).Width,
            DecodePixelHeight = (int)(~this).Height,
            DecodePixelType = DecodePixelType.Logical,
        };

        (~this).ImageOpened += OnImageOpened;
        (~this).PointerExited += OnImagePointerExited;
        (~this).PointerEntered += OnImagePointerEntered;
        (~this).PointerPressed += OnImagePointerPressed;
    }

    unsafe static void OnImagePointerPressed(object sender, RoutedEventArgs args)
    {
        fixed (char* lpFile = (string)((Image)sender).Tag)
            ShellExecute(Null, null, lpFile, null, null, SW_NORMAL);
    }

    static void OnImageOpened(object sender, RoutedEventArgs args) => ((Image)sender).Visibility = Visibility.Visible;
    static void OnImagePointerEntered(object sender, RoutedEventArgs args) => CoreWindow.GetForCurrentThread().PointerCursor = _hand;
    static void OnImagePointerExited(object sender, RoutedEventArgs args) => CoreWindow.GetForCurrentThread().PointerCursor = _arrow;
}
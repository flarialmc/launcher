using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Flarial.Runtime.Services;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;

namespace Flarial.Launcher.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        Initialized += OnInitialized;
    }

    async void OnInitialized(object? sender, EventArgs args)
    {
        Initialized -= OnInitialized;

        foreach (var promotion in await PromotionService.GetAsync()) Dispatcher.Post(async () =>
        {
            try
            {
                using var stream = await promotion.GetAsync();

                Image image = new()
                {
                    Width = 320 * 0.8,
                    Height = 50 * 0.8,
                    Tag = promotion.Uri,
                    Source = new Bitmap(stream),
                    Cursor = new Cursor(StandardCursorType.Hand)
                };

                image.PointerPressed += OnPointerPressed;
                RenderOptions.SetBitmapInterpolationMode(image, BitmapInterpolationMode.HighQuality);

                Promotions.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
                Grid.SetColumn(image, Promotions.ColumnDefinitions.Count - 1);

                Promotions.Children.Add(image);
            }
            catch { }
        }, DispatcherPriority.Send);
    }

    static unsafe async void OnPointerPressed(object? sender, RoutedEventArgs args)
    {
        fixed (char* uri = (string)((Image)sender!).Tag!)
            ShellExecute(new(), null, uri, null, null, SW_NORMAL);
    }
}
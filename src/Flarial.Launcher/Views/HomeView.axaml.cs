using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Flarial.Runtime.Services;
using Flarial.Runtime.Unmanaged;

namespace Flarial.Launcher.Views;

public partial class HomeView : UserControl
{
    static readonly Cursor s_cursor = new(StandardCursorType.Hand);

    public HomeView()
    {
        InitializeComponent();
    }

    async void OnInitialized(object? sender, EventArgs args)
    {
        Initialized -= OnInitialized;

        _ = Task.Run(async () =>
        {
            foreach (var promotion in await PromotionService.GetAsync()) Dispatcher.Post(async () =>
            {
                try
                {
                    using var stream = await promotion.GetImageAsync();

                    Image image = new()
                    {
                        Width = 320 * 0.8,
                        Height = 50 * 0.8,
                        Cursor = s_cursor,
                        Tag = promotion.Uri,
                        Source = new Bitmap(stream)
                    };

                    image.PointerPressed += OnPointerPressed;
                    RenderOptions.SetBitmapInterpolationMode(image, BitmapInterpolationMode.HighQuality);

                    Promotions.Children.Add(image);
                }
                catch { }
            }, DispatcherPriority.Background);
        });
    }

    static void OnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (sender is not Control control)
            return;

        var file = control.Tag as string;
        var point = args.GetCurrentPoint(control);

        if (!point.Properties.IsLeftButtonPressed) return;
        if (file is { }) NativeMethods.ShellExecute(file);
    }
}
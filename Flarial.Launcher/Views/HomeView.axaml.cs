using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Flarial.Runtime.Services;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Flarial.Launcher.Views;

public partial class HomeView : UserControl
{
    static readonly HttpClient s_httpClient = new();
    Uri? _adUri;

    public HomeView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void LaunchButton_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }

    async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        await Task.Delay(TimeSpan.FromSeconds(3));
        await ShowPromotionAsync();
    }

    async Task ShowPromotionAsync()
    {
        var promotions = await PromotionManager.GetAsync();
        if (promotions.Length == 0) return;

        var promotion = promotions[0];
        if (!Uri.TryCreate(promotion.Uri, UriKind.Absolute, out _adUri))
            return;

        await using var stream = await s_httpClient.GetStreamAsync(promotion.Image);
        AdImage.Source = new Bitmap(stream);
        AdPopup.IsOpen = true;

        if (AdBorder.RenderTransform is not TranslateTransform transform)
            return;

        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(350),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters = { new Setter(TranslateTransform.YProperty, 0d) }
                }
            }
        };

        await animation.RunAsync(AdBorder);
    }

    void AdBorder_OnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (_adUri is null) return;

        Process.Start(new ProcessStartInfo
        {
            FileName = _adUri.ToString(),
            UseShellExecute = true
        });
    }
}

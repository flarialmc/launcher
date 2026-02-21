using Flarial.Launcher.Xaml;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Flarial.Launcher.Controls;

abstract class PromotionImageButton : XamlElement<Image>
{
    protected abstract string ImageUri { get; }
    protected abstract string NavigateUri { get; }

    static readonly CoreCursor _hand = new(CoreCursorType.Hand, 0);
    static readonly CoreCursor _arrow = new(CoreCursorType.Arrow, 0);

    protected PromotionImageButton() : base(new())
    {
        (~this).Width = 320 * 0.95;
        (~this).Height = 50 * 0.95;
        (~this).VerticalAlignment = VerticalAlignment.Bottom;

        (~this).Source = new BitmapImage
        {
            UriSource = new(ImageUri),
            DecodePixelType = DecodePixelType.Logical
        };

        (~this).ImageOpened += OnImageOpened;
        (~this).PointerPressed += OnImagePointerPressed;

        (~this).PointerEntered += OnImagePointerEntered;
        (~this).PointerExited += OnImagePointerExited;

        (~this).Tag = NavigateUri;
    }

    static void OnImageOpened(object sender, RoutedEventArgs args)
    {
        var image = (Image)sender;
        image.Visibility = Visibility.Visible;
    }

    static void OnImagePointerPressed(object sender, RoutedEventArgs args)
    {
        var image = (Image)sender;
        NativeMethods.ShellExecute((string)image.Tag);
    }

    static void OnImagePointerEntered(object sender, RoutedEventArgs args)
    {
        var window = CoreWindow.GetForCurrentThread();
        window.PointerCursor = _hand;
    }

    static void OnImagePointerExited(object sender, RoutedEventArgs args)
    {
        var window = CoreWindow.GetForCurrentThread();
        window.PointerCursor = _arrow;
    }
}

sealed class LiteByteHostingImageButton : PromotionImageButton
{
    protected override string ImageUri => "https://litebyte.co/images/flarial.png";
    protected override string NavigateUri => "https://litebyte.co/minecraft?utm_source=flarial-client&utm_medium=app&utm_campaign=bedrock-launch";
}

sealed class CollapseNetworkImageButton : PromotionImageButton
{
    protected override string ImageUri => "https://collapsemc.com/assets/other/ad-banner.png";
    protected override string NavigateUri => "minecraft://?addExternalServer=Collapse|clps.gg:19132";
}

sealed class InfinityNetworkImageButton : PromotionImageButton
{
    protected override string ImageUri => "https://assets.infinitymcpe.com/banner.png";
    protected override string NavigateUri => "https://discord.gg/infinitymcpe";
}
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Controls;

sealed class PromotionImagesBox : Grid
{
    readonly PromotionImageButton _left = new LiteByteHostingImageButton();
    readonly PromotionImageButton _middle = new StubImageButton();
    readonly PromotionImageButton _right = new InfinityNetworkImageButton();

    internal PromotionImagesBox()
    {
        Margin = new(12);
        RowSpacing = 12;
        ColumnSpacing = 12;

        ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        ColumnDefinitions.Add(new() { Width = GridLength.Auto });

        SetColumn(~_left, 0);
        SetColumn(~_middle, 1);
        SetColumn(~_right, 2);

        Children.Add(~_left);
        Children.Add(~_right);
        Children.Add(~_middle);
    }
}
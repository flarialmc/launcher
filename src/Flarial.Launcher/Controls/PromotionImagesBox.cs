using Flarial.Runtime.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Controls;

sealed class PromotionImagesBox : Grid
{
    internal PromotionImagesBox(Promotion[] promotions)
    {
        Margin = new(12);
        RowSpacing = 12;
        ColumnSpacing = 12;

        foreach (var promotion in promotions)
        {
            Image image = ~new PromotionImage(promotion);

            Children.Add(image);
            ColumnDefinitions.Add(new() { Width = GridLength.Auto });

            SetColumn(image, ColumnDefinitions.Count - 1);
        }
    }
}
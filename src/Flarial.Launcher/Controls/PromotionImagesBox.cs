using System.Linq;
using Flarial.Runtime.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Controls;

sealed class PromotionImagesBox : Grid
{
    internal PromotionImagesBox()
    {
        Margin = new(12);
        ColumnSpacing = 9;
        Loading += OnLoading;
    }

    static async void OnLoading(FrameworkElement sender, object args)
    {
        var grid = (Grid)sender;
        var promotions = await PromotionRegistry.GetAsync();

        foreach (var promotion in promotions)
        {
            var image = ~new PromotionImage(promotion);

            grid.Children.Add(image);
            grid.ColumnDefinitions.Add(new() { Width = GridLength.Auto });

            SetColumn(image, grid.ColumnDefinitions.Count - 1);
        }
    }
}
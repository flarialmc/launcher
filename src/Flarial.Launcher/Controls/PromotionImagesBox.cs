using System.Collections.Generic;
using Flarial.Runtime.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Controls;

sealed class PromotionImagesBox : Grid
{
    internal PromotionImagesBox()
    {
        Margin = new(12);

        RowSpacing = 12;
        ColumnSpacing = 12;

        Loading += OnLoading;
    }

    async void OnLoading(FrameworkElement sender, object args)
    {
        Loading -= OnLoading;
        
        foreach (var promotion in await PromotionManager.GetAsync())
        {
            var image = ~new PromotionImage(promotion);

            ColumnDefinitions.Add(new() { Width = GridLength.Auto });
            SetColumn(image, ColumnDefinitions.Count - 1);

            Children.Add(image);
        }
    }
}
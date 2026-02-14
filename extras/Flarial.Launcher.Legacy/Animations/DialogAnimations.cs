using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Flarial.Launcher.Animations;

public static class DialogAnimations
{
    private static async void BlurElement(FrameworkElement control, double fromRadius, double toRadius, TimeSpan duration)
    {
        var animation = new DoubleAnimation
        {
            From = fromRadius,
            To = toRadius,
            Duration = duration
        };
        control.Effect = new BlurEffect();
        control.Effect.BeginAnimation(BlurEffect.RadiusProperty, animation);

        if (toRadius != 0) return;
        await Task.Delay(duration);
        control.Effect = null;
    }
    
    public static void OpenDialog(FrameworkElement mainGrid, FrameworkElement dialogGrid)
    {
        dialogGrid.Visibility = Visibility.Visible;
        
        BlurElement(mainGrid, 0, 16, TimeSpan.FromMilliseconds(250));

        var animation = new DoubleAnimation
        {
            From = 0.0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(250),
        };
        
        dialogGrid.BeginAnimation(UIElement.OpacityProperty, animation);
    }
    
    public static void CloseDialog(FrameworkElement mainGrid, FrameworkElement dialogGrid)
    {
        dialogGrid.Visibility = Visibility.Visible;
        
        BlurElement(mainGrid, 16, 0, TimeSpan.FromMilliseconds(250));

        var animation = new DoubleAnimation
        {
            From = 1,
            To = 0.0,
            Duration = TimeSpan.FromMilliseconds(250),
        };
        
        dialogGrid.BeginAnimation(UIElement.OpacityProperty, animation);
        animation.Completed += (_, _) =>
        {
            dialogGrid.Visibility = Visibility.Collapsed;
        };
    }
}
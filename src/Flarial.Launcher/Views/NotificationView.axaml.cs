using System;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Flarial.Launcher.Controls.AcrylicBlur;
using Flarial.Launcher.ViewModels;

namespace Flarial.Launcher.Views;

public partial class NotificationView : UserControl
{
    NotificationViewModel? _subscribedViewModel;

    public NotificationView()
    {
        InitializeComponent();

        DataContextChanged += (_, _) => SubscribeViewModel();
        PlayEnterAnimation();
    }

    void SubscribeViewModel()
    {
        if (_subscribedViewModel is not null)
            _subscribedViewModel.CloseRequested -= PlayExitAnimation;

        _subscribedViewModel = DataContext as NotificationViewModel;

        if (_subscribedViewModel is not null)
            _subscribedViewModel.CloseRequested += PlayExitAnimation;
    }

    private async void PlayEnterAnimation()
    {
        var card = this.FindControl<AcrylicBlur>("Card")!;
        var translate = (TranslateTransform)card.RenderTransform!;

        card.Opacity = 0;
        translate.X  = 120;

        var anim = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(300),
            FillMode = FillMode.Forward,
            Easing   = new CubicEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(OpacityProperty, 0d),
                        new Setter(TranslateTransform.XProperty, 120d),
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(OpacityProperty, 1d),
                        new Setter(TranslateTransform.XProperty, 0d),
                    }
                },
            }
        };

        await anim.RunAsync(card);
    }

    private async void PlayExitAnimation(NotificationViewModel vm)
    {
        var card = this.FindControl<AcrylicBlur>("Card")!;
        
        var fadeAnim = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(200),
            FillMode = FillMode.Forward,
            Easing   = new CubicEaseIn(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(OpacityProperty, 1d),
                        new Setter(TranslateTransform.XProperty, 0d),
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(OpacityProperty, 0d),
                        new Setter(TranslateTransform.XProperty, 60d),
                    }
                },
            }
        };

        await fadeAnim.RunAsync(card);
        
        var startHeight = card.Bounds.Height + card.Margin.Top + card.Margin.Bottom;
        var collapseAnim = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(180),
            FillMode = FillMode.Forward,
            Easing   = new CubicEaseInOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(HeightProperty, startHeight) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(HeightProperty, 0d) }
                },
            }
        };
        
        card.Height = startHeight;
        await collapseAnim.RunAsync(card);
        
        vm.CompleteDismiss();
    }
}

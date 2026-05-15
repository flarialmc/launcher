using System;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Flarial.Launcher.Controls.AcrylicBlur;
using Flarial.Launcher.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace Flarial.Launcher.Views;

public partial class MessageBoxView : ReactiveUserControl<MessageBoxViewModel>
{
    public MessageBoxView()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            Dispatcher.UIThread.Post(PlayEnterAnimation, DispatcherPriority.Render);

            this.WhenAnyValue(x => x.ViewModel)
                .WhereNotNull()
                .SelectMany(vm => vm.CloseRequested.Select(_ => vm))
                .Subscribe(PlayExitAnimation)
                .DisposeWith(disposables);
        });
    }

    private async void PlayEnterAnimation()
    {
        var panel = this.FindControl<AcrylicBlur>("DialogPanel")!;
        var scrim = this.FindControl<Panel>("Scrim")!;
        var scale = (ScaleTransform)panel.RenderTransform!;
        
        panel.Opacity = 0;
        scrim.Opacity = 0;
        scale.ScaleX  = 0.95;
        scale.ScaleY  = 0.95;

        var panelAnim = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(200),
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
                        new Setter(ScaleTransform.ScaleXProperty, 0.95d),
                        new Setter(ScaleTransform.ScaleYProperty, 0.95d),
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(OpacityProperty, 1d),
                        new Setter(ScaleTransform.ScaleXProperty, 1d),
                        new Setter(ScaleTransform.ScaleYProperty, 1d),
                    }
                },
            }
        };

        var scrimAnim = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(150),
            FillMode = FillMode.Forward,
            Easing   = new CubicEaseOut(),
            Children =
            {
                new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(OpacityProperty, 0d) } },
                new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(OpacityProperty, 1d) } },
            }
        };

        await Task.WhenAll(
            panelAnim.RunAsync(panel),
            scrimAnim.RunAsync(scrim));
    }

    private async void PlayExitAnimation(MessageBoxViewModel vm)
    {
        var panel = this.FindControl<AcrylicBlur>("DialogPanel")!;
        var scrim = this.FindControl<Panel>("Scrim")!;

        var panelAnim = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(150),
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
                        new Setter(ScaleTransform.ScaleXProperty, 1d),
                        new Setter(ScaleTransform.ScaleYProperty, 1d),
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(OpacityProperty, 0d),
                        new Setter(ScaleTransform.ScaleXProperty, 0.95d),
                        new Setter(ScaleTransform.ScaleYProperty, 0.95d),
                    }
                },
            }
        };

        var scrimAnim = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(150),
            FillMode = FillMode.Forward,
            Easing   = new CubicEaseIn(),
            Children =
            {
                new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(OpacityProperty, 1d) } },
                new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(OpacityProperty, 0d) } },
            }
        };

        await Task.WhenAll(
            panelAnim.RunAsync(panel),
            scrimAnim.RunAsync(scrim));

        vm.CompleteClose();
    }
}
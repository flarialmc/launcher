using System;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Flarial.Launcher.Types;
using Flarial.Launcher.ViewModels;
using ReactiveUI;

namespace Flarial.Launcher.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
        
        MessageBus.Current.Listen<PageTransitions>()
            .Subscribe(PageTransition);
    }

    private double _currentPageY;

    private async void PageTransition(PageTransitions page)
    {
        var selectedPageY = page switch
        {
            PageTransitions.SettingsGeneralPage => 0,
            PageTransitions.SettingsVersionsPage => 500,
            PageTransitions.SettingsConfigsPage => 1000,
            _ => _currentPageY
        };

        if (Math.Abs(selectedPageY - _currentPageY) < 1)
            return;

        if (DataContext is not SettingsViewModel settingsViewModel) return;
        settingsViewModel.IsAnimating = true;
            
        UserControlGrid.IsEnabled = false;

        if (UserControlGrid.RenderTransform is not ScaleTransform) return;
        
        var zoomOut = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.2),
            Easing = new QuadraticEaseOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 0.9),
                        new Setter(ScaleTransform.ScaleYProperty, 0.9),
                    }
                }
            }
        };

        var generalMove = CreateMove(0 - selectedPageY);
        var versionsMove = CreateMove(500 - selectedPageY);
        var configsMove = CreateMove(1000 - selectedPageY);
        
        var zoomIn = new Animation
        {
            Delay = TimeSpan.FromMilliseconds(500),
            Duration = TimeSpan.FromSeconds(0.2),
            Easing = new QuadraticEaseIn(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 1.0),
                        new Setter(ScaleTransform.ScaleYProperty, 1.0),
                    }
                }
            }
        };
        
        _ = zoomOut.RunAsync(UserControlGrid);
        _ = generalMove.RunAsync(SettingsGeneralViewControl);
        _ = versionsMove.RunAsync(SettingsVersionsViewControl);
        _ = configsMove.RunAsync(SettingsConfigsViewControl);
        _ = zoomIn.RunAsync(UserControlGrid);

        await Task.Delay(700);

        UserControlGrid.IsEnabled = true;
        _currentPageY = selectedPageY;
        settingsViewModel.IsAnimating = false;
        return;

        Animation CreateMove(double toY) => new()
        {
            Delay = TimeSpan.FromMilliseconds(250),
            Duration = TimeSpan.FromSeconds(0.2),
            Easing = new QuadraticEaseInOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters = { new Setter(TranslateTransform.YProperty, toY) }
                }
            }
        };
    }
}
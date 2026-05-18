using System;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Flarial.Launcher.Types;
using Flarial.Launcher.ViewModels;

namespace Flarial.Launcher.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        
        AppMessageBus.PageTransitionRequested += PageTransition;
    }

    private int _currentPageIndex;

    private async void PageTransition(PageTransitions page)
    {
        var selectedPageIndex = page switch
        {
            PageTransitions.SettingsGeneralPage => 0,
            PageTransitions.SettingsVersionsPage => 1,
            PageTransitions.SettingsConfigsPage => 2,
            _ => _currentPageIndex
        };

        if (selectedPageIndex == _currentPageIndex)
            return;

        if (DataContext is not SettingsViewModel settingsViewModel) return;
        settingsViewModel.IsAnimating = true;
            
        UserControlGrid.IsEnabled = false;

        if (UserControlGrid.RenderTransform is not ScaleTransform) return;

        var pageHeight = GetPageHeight();
        var selectedPageY = selectedPageIndex * pageHeight;
        
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
        var versionsMove = CreateMove(pageHeight - selectedPageY);
        var configsMove = CreateMove(pageHeight * 2 - selectedPageY);
        
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
        _currentPageIndex = selectedPageIndex;
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

    private void UserControlGrid_OnSizeChanged(object? sender, SizeChangedEventArgs e) => PositionPages();

    private void PositionPages()
    {
        var pageHeight = GetPageHeight();
        SetPageOffset(SettingsGeneralViewControl, -_currentPageIndex * pageHeight);
        SetPageOffset(SettingsVersionsViewControl, (1 - _currentPageIndex) * pageHeight);
        SetPageOffset(SettingsConfigsViewControl, (2 - _currentPageIndex) * pageHeight);
    }

    private double GetPageHeight() => Math.Max(1, UserControlGrid.Bounds.Height);

    private static void SetPageOffset(Control control, double y)
    {
        if (control.RenderTransform is TranslateTransform translate)
            translate.Y = y;
    }
}

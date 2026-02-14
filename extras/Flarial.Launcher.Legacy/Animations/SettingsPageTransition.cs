using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows;

namespace Flarial.Launcher.Animations
{
    public static class SettingsPageTransition
    {
        public static void SettingsEnterAnimation(Border b, Grid g)
        {
            var borderStoryboard = new Storyboard();
            var gridStoryboard = new Storyboard();

            var setBorderMargin = new ThicknessAnimation
            {
                To = new Thickness(15, 15, 15, 15),
                Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            var moveGrid = new ThicknessAnimation
            {
                To = new Thickness(0, -500, 0, 0),
                Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                BeginTime = new TimeSpan(0, 0, 0, 0, 200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            borderStoryboard.Children.Add(setBorderMargin);
            gridStoryboard.Children.Add(moveGrid);

            Storyboard.SetTargetName(setBorderMargin, b.Name);
            Storyboard.SetTargetName(moveGrid, g.Name);

            Storyboard.SetTargetProperty(setBorderMargin, new PropertyPath(FrameworkElement.MarginProperty));
            Storyboard.SetTargetProperty(moveGrid, new PropertyPath(FrameworkElement.MarginProperty));

            borderStoryboard.Begin(b);
            gridStoryboard.Begin(g);
        }

        public static void SettingsLeaveAnimation(Border border, Grid grid)
        {
            if (!MainWindow.Reverse)
            {
                var borderMarginAnimation = new ThicknessAnimation
                {
                    To = new Thickness(0, 0, 0, 0),
                    Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                    BeginTime = new TimeSpan(0, 0, 0, 0, 250),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };

                var borderStoryboard = new Storyboard();

                borderStoryboard.Children.Add(borderMarginAnimation);
                Storyboard.SetTargetName(borderMarginAnimation, border.Name);
                Storyboard.SetTargetProperty(borderMarginAnimation, new PropertyPath(FrameworkElement.MarginProperty));
                borderStoryboard.Begin(border);
            }

            var gridMarginAnimation = new ThicknessAnimation
            {
                To = new Thickness(0, 0, 0, 0),
                Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            var gridStoryboard = new Storyboard();

            gridStoryboard.Children.Add(gridMarginAnimation);
            Storyboard.SetTargetName(gridMarginAnimation, grid.Name);
            Storyboard.SetTargetProperty(gridMarginAnimation, new PropertyPath(FrameworkElement.MarginProperty));
            gridStoryboard.Begin(grid);
        }

        public static void SettingsNavigateAnimation(double position, Border border, StackPanel stackPanel)
        {
            border.IsEnabled = false;
            stackPanel.IsEnabled = false;

            // Does a zoom out type effect by making the borer for the pages smaller
            var zoomInOutStoryboard = new Storyboard();

            var zoomOutXDoubleAnimation = new DoubleAnimation
            {
                To = 0.9,
                From = 1.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var zoomOutYDoubleAnimation = zoomOutXDoubleAnimation.Clone();

            zoomInOutStoryboard.Children.Add(zoomOutXDoubleAnimation);
            zoomInOutStoryboard.Children.Add(zoomOutYDoubleAnimation);
            Storyboard.SetTargetProperty(zoomOutXDoubleAnimation, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTarget(zoomOutXDoubleAnimation, border);
            Storyboard.SetTargetProperty(zoomOutYDoubleAnimation, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(zoomOutYDoubleAnimation, border);

            // Moves the stack panel so the right page shows
            var moveStoryBoard = new Storyboard();

            var moveThicknessAnimation = new ThicknessAnimation
            {
                To = new Thickness(0, position, 0, 0),
                Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut },
                BeginTime = new TimeSpan(0, 0, 0, 0, 250)
            };

            moveStoryBoard.Children.Add(moveThicknessAnimation);
            Storyboard.SetTarget(moveThicknessAnimation, stackPanel);
            Storyboard.SetTargetProperty(moveThicknessAnimation, new PropertyPath(FrameworkElement.MarginProperty));

            // Zooms back in

            var zoomInXDoubleAnimation = new DoubleAnimation
            {
                To = 1.0,
                From = 0.9,
                Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn },
                BeginTime = new TimeSpan(0, 0, 0, 0, 500)
            };

            var zoomInYDoubleAnimation = zoomInXDoubleAnimation.Clone();

            zoomInOutStoryboard.Children.Add(zoomInXDoubleAnimation);
            zoomInOutStoryboard.Children.Add(zoomInYDoubleAnimation);
            Storyboard.SetTarget(zoomInXDoubleAnimation, border);
            Storyboard.SetTargetProperty(zoomInXDoubleAnimation, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTarget(zoomInYDoubleAnimation, border);
            Storyboard.SetTargetProperty(zoomInYDoubleAnimation, new PropertyPath("RenderTransform.ScaleY"));

            // Starts the storyboards
            moveStoryBoard.Begin(stackPanel);
            zoomInOutStoryboard.Begin();

            border.IsEnabled = true;
            stackPanel.IsEnabled = true;
        }
    }
}

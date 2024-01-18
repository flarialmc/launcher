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
    public static class ToggleButtonTransitions
    {
        public static void CheckedAnimation(Grid grid)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                To = 35,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            var storyboard = new Storyboard();

            storyboard.Children.Add(doubleAnimation);
            Storyboard.SetTargetName(doubleAnimation, grid.Name);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Grid.HeightProperty));
            storyboard.Begin(grid);
        }

        public static void UnCheckedAnimation(Grid grid)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                To = 0,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            var storyboard = new Storyboard();

            storyboard.Children.Add(doubleAnimation);
            Storyboard.SetTargetName(doubleAnimation, grid.Name);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Grid.HeightProperty));
            storyboard.Begin(grid);
        }
    }
}

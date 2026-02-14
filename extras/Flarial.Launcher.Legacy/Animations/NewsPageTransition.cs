using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Shapes;

namespace Flarial.Launcher.Animations
{
    public static class NewsPageTransition
    {
        public static void Animation(bool reverse, Border b1, Border b2, Path na)
        {
            if (!reverse)
            {
                ThicknessAnimation myDoubleAnimation = new ThicknessAnimation();
                myDoubleAnimation.From = new Thickness(0, 0, 0, 0);
                myDoubleAnimation.To = new Thickness(15, 15, 15, 15);
                myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                myDoubleAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

                DoubleAnimation myDoubleAnimation4 = new DoubleAnimation();
                myDoubleAnimation4.From = 0;
                myDoubleAnimation4.To = 180;
                myDoubleAnimation4.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                myDoubleAnimation4.BeginTime = new TimeSpan(0, 0, 0, 0, 200);
                myDoubleAnimation4.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

                Storyboard myStoryboard = new Storyboard();

                myStoryboard.Children.Add(myDoubleAnimation);
                Storyboard.SetTargetName(myDoubleAnimation, b1.Name);
                Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(Border.MarginProperty));

                Storyboard myStoryboard3 = new Storyboard();

                myStoryboard3.Children.Add(myDoubleAnimation4);
                Storyboard.SetTarget(myDoubleAnimation4, na);
                Storyboard.SetTargetProperty(myDoubleAnimation4, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

                myStoryboard.Begin(b1);
                myStoryboard3.Begin(na);

                DoubleAnimation myDoubleAnimation2 = new DoubleAnimation();
                myDoubleAnimation2.From = 0;
                myDoubleAnimation2.To = 385;
                myDoubleAnimation2.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                myDoubleAnimation2.BeginTime = new TimeSpan(0, 0, 0, 0, 200);
                myDoubleAnimation2.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

                ThicknessAnimation myDoubleAnimation3 = new ThicknessAnimation();
                myDoubleAnimation3.From = new Thickness(0, 0, 0, 0);
                myDoubleAnimation3.To = new Thickness(0, 15, 15, 15);
                myDoubleAnimation3.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                myDoubleAnimation3.BeginTime = new TimeSpan(0, 0, 0, 0, 200);
                myDoubleAnimation3.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

                Storyboard myStoryboard2 = new Storyboard();
                myStoryboard2.Children.Add(myDoubleAnimation2);
                Storyboard.SetTargetName(myDoubleAnimation2, b2.Name);
                Storyboard.SetTargetProperty(myDoubleAnimation2, new PropertyPath(Border.WidthProperty));

                myStoryboard2.Children.Add(myDoubleAnimation3);
                Storyboard.SetTargetName(myDoubleAnimation3, b2.Name);
                Storyboard.SetTargetProperty(myDoubleAnimation3, new PropertyPath(Border.MarginProperty));

                myStoryboard2.Begin(b2);
                MainWindow.Reverse = true;
            }
            else
            {
                ThicknessAnimation myDoubleAnimation = new ThicknessAnimation();
                myDoubleAnimation.From = new Thickness(15, 15, 15, 15);
                myDoubleAnimation.To = new Thickness(0, 0, 0, 0);
                myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                myDoubleAnimation.BeginTime = new TimeSpan(0, 0, 0, 0, 200);
                myDoubleAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

                DoubleAnimation myDoubleAnimation4 = new DoubleAnimation();
                myDoubleAnimation4.From = 180;
                myDoubleAnimation4.To = 0;
                myDoubleAnimation4.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                myDoubleAnimation4.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

                Storyboard myStoryboard = new Storyboard();

                myStoryboard.Children.Add(myDoubleAnimation);
                Storyboard.SetTargetName(myDoubleAnimation, b1.Name);
                Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(Border.MarginProperty));

                Storyboard myStoryboard3 = new Storyboard();

                myStoryboard3.Children.Add(myDoubleAnimation4);
                Storyboard.SetTarget(myDoubleAnimation4, na);
                Storyboard.SetTargetProperty(myDoubleAnimation4, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

                myStoryboard.Begin(b1);
                myStoryboard3.Begin(na);

                DoubleAnimation myDoubleAnimation2 = new DoubleAnimation();
                myDoubleAnimation2.From = 385;
                myDoubleAnimation2.To = 0;
                myDoubleAnimation2.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                myDoubleAnimation2.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

                ThicknessAnimation myDoubleAnimation3 = new ThicknessAnimation();
                myDoubleAnimation3.From = new Thickness(0, 15, 15, 15);
                myDoubleAnimation3.To = new Thickness(0, 0, 0, 0);
                myDoubleAnimation3.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                myDoubleAnimation3.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

                Storyboard myStoryboard2 = new Storyboard();
                myStoryboard2.Children.Add(myDoubleAnimation2);
                Storyboard.SetTargetName(myDoubleAnimation2, b2.Name);
                Storyboard.SetTargetProperty(myDoubleAnimation2, new PropertyPath(Border.WidthProperty));

                myStoryboard2.Children.Add(myDoubleAnimation3);
                Storyboard.SetTargetName(myDoubleAnimation3, b2.Name);
                Storyboard.SetTargetProperty(myDoubleAnimation3, new PropertyPath(Border.MarginProperty));

                myStoryboard2.Begin(b2);
                MainWindow.Reverse = false;
            }
        }
    }
}

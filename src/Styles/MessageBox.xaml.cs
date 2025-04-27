using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Flarial.Launcher.Styles
{
    public partial class MessageBox : UserControl
    {
        private readonly int _lifetime;

        public MessageBox(string text, int lifetime = 0, bool autoCalculateLifetime = false)
        {
            InitializeComponent();

            ((TextBlock)this.FindName("MessageText")).Text = text;

            // Calculate Lifetime
            if (autoCalculateLifetime && !string.IsNullOrEmpty(text))
            {
                _lifetime = 1000 + text.Length * 50;
            }
            else
            {
                _lifetime = lifetime;
            }

            // Start the lifetime timer
            if (_lifetime > 0)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(_lifetime);
                    Dispatcher.Invoke(() => TriggerHideAnimation());
                });
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.IsEnabled = false;
            }

            TriggerHideAnimation();
        }

        private void TriggerHideAnimation()
        {
            var hideAnimation = (Storyboard)FindResource("HideAnimation");

            // Add an event handler to remove the MessageBox after the animation completes
            void onAnimationCompleted(object s, EventArgs args)
            {
                hideAnimation.Completed -= onAnimationCompleted; // Unsubscribe to avoid memory leaks
                if (this.Parent is Panel parent)
                {
                    parent.Children.Remove(this);
                }
            }

            hideAnimation.Completed += onAnimationCompleted;

            hideAnimation.Begin(this);
        }
    }
}
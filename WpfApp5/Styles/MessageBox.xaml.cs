using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Flarial.Launcher.Styles
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : UserControl
    {
        public string Text { get; set; }

        public MessageBox()
        {
            InitializeComponent();
            this.DataContext = this;
            Text = "temp";
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var sb = new Storyboard();

            var an1 = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(200),
                To = 0,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            var an2 = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(200),
                To = 0,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            var an3 = new ThicknessAnimation()
            {
                Duration = TimeSpan.FromMilliseconds(200),
                To = new Thickness(0, 0, 0, 0),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            Storyboard.SetTarget(an1, this);
            Storyboard.SetTargetProperty(an1, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTarget(an2, this);
            Storyboard.SetTargetProperty(an2, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(an3, this);
            Storyboard.SetTargetProperty(an3, new PropertyPath(MarginProperty));

            sb.Children.Add(an1);
            sb.Children.Add(an2);
            sb.Children.Add(an3);

            sb.Begin(this);

            await Task.Delay(200);
            ((StackPanel)this.Parent).Children.Remove(this);
        }
    }
}

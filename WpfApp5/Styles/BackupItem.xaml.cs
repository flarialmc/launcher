using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Flarial.Launcher.Managers;

namespace Flarial.Launcher.Styles;

public partial class BackupItem : UserControl
{
    public string Time { get; set; }
    public string Path { get; set; }
    
    public BackupItem()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void LoadBackup(object sender, RoutedEventArgs e)
    {
        Dispatcher.InvokeAsync(async () => await BackupManager.LoadBackup(Time));
        Application.Current.Dispatcher.Invoke(() =>
        {
            MainWindow.CreateMessageBox("Loading the backup. This may take some time!");
            MainWindow.CreateMessageBox("Don't launch Minecraft in the mean time.");

        });
        //add code here
    }

    private async void DeleteBackup(object sender, RoutedEventArgs e)
    {
        // add the code here

        var animationX = new DoubleAnimation
        {
            To = 0,
            EasingFunction = new QuadraticEase{ EasingMode = EasingMode.EaseIn},
            Duration = TimeSpan.FromMilliseconds(250)
        };
        var animationY = animationX.Clone();
        
        var storyboard = new Storyboard();
        
        Storyboard.SetTarget(animationX, this);
        Storyboard.SetTargetProperty(animationX, new PropertyPath("RenderTransform.ScaleX"));
        Storyboard.SetTarget(animationY, this);
        Storyboard.SetTargetProperty(animationY, new PropertyPath("RenderTransform.ScaleY"));
        
        storyboard.Children.Add(animationX);
        storyboard.Children.Add(animationY);
        
        storyboard.Begin(this);

        await Task.Delay(animationX.Duration.TimeSpan);
        
        (this.VisualParent as VirtualizingStackPanel)?.Children.Remove(this);
        
        await Dispatcher.InvokeAsync(async () => await BackupManager.DeleteBackup(Time));

    }
}
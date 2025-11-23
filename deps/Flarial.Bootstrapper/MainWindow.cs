using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

sealed class MainWindow : Window
{
    readonly Grid _grid = new()
    {
        Background = new ImageBrush()
        {
            ImageSource = EmbeddedResources.GetImage("Background.png"),
            Stretch = Stretch.UniformToFill
        }
    };

    readonly Image _image = new()
    {
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Source = EmbeddedResources.GetImage("Application.ico")
    };

    readonly ProgressBar _progressBar = new()
    {
        IsIndeterminate = true
    };

    internal MainWindow()
    {
        Content = _grid;
        Title = "Flarial Bootstrapper";
        Icon = EmbeddedResources.GetImage("Application.ico");

        UseLayoutRounding = true;
        SnapsToDevicePixels = true;
        Width = 1280; Height = 720;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        SizeToContent = SizeToContent.Manual;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        _image.Width = _image.Source.Width / 1.025;
        _image.Height = _image.Source.Height / 1.025;

        _grid.RowDefinitions.Add(new());
        _grid.RowDefinitions.Add(new() { Height = GridLength.Auto });

        Grid.SetRow(_image, 0); Grid.SetColumn(_image, 0);
        Grid.SetRow(_progressBar, 1); Grid.SetColumn(_progressBar, 0);

        _grid.Children.Add(_image);
        _grid.Children.Add(_progressBar);
    }

    protected override void OnClosing(CancelEventArgs args){base.OnClosing(args); args.Cancel = true;}

    protected override async void OnContentRendered(EventArgs args)
    {
        base.OnContentRendered(args);
        await GameLaunchHelper.LaunchAsync();
        Application.Current.Shutdown();
    }
}
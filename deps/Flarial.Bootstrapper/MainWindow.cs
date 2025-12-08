using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

sealed class MainWindow : Window
{
    readonly Grid _grid = new()
    {
        Background = new ImageBrush()
        {
            ImageSource = EmbeddedResources.GetImage("Background.jpg"),
            Stretch = Stretch.UniformToFill
        }
    };

    readonly Image _image = new()
    {
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Source = EmbeddedResources.GetImage("Application.ico")
    };

    readonly ProgressBar _progressBar = new() { IsIndeterminate = true };

    GameLaunchHelper.Request? _request = null;

    internal MainWindow()
    {
        Content = _grid;
        Title = "Flarial Bootstrapper";
        Icon = EmbeddedResources.GetImage("Application.ico");

        UseLayoutRounding = true;
        SnapsToDevicePixels = true;
        Width = 1280; Height = 720;
        WindowStyle = WindowStyle.ThreeDBorderWindow;
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

    protected override void OnClosing(CancelEventArgs args)
    {
        base.OnClosing(args);
        _request?.Cancel();
    }

    protected override async void OnContentRendered(EventArgs args)
    {
        base.OnContentRendered(args);
        _request = GameLaunchHelper.Launch();
        await _request; Close();
    }
}
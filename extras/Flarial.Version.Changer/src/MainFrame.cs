using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Flarial.Launcher.Interface;
using Flarial.Launcher.Services.Game;
using Flarial.Launcher.Services.Versions;
using ModernWpf.Controls.Primitives;

sealed class MainFrame : Grid
{
    readonly ListBox _listBox = new()
    {
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new(0, 0, 0, 12),
        IsEnabled = false
    };

    readonly Button _button = new()
    {
        Content = "Install",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Visibility = Visibility.Hidden
    };

    readonly TextBlock _textBlock = new()
    {
        Text = "Connecting...",
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center
    };

    readonly ModernWpf.Controls.ProgressBar _progressBar = new()
    {
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        IsIndeterminate = true
    };

    Task? _task;

    internal MainFrame()
    {
        Margin = new(12);
        RowDefinitions.Add(new());
        RowDefinitions.Add(new() { Height = GridLength.Auto });

        VirtualizingPanel.SetIsVirtualizing(_listBox, true);
        ScrollViewerHelper.SetAutoHideScrollBars(_listBox, true);
        ScrollViewer.SetIsDeferredScrollingEnabled(_listBox, true);
        VirtualizingPanel.SetIsContainerVirtualizable(_listBox, true);
        VirtualizingPanel.SetIsVirtualizingWhenGrouping(_listBox, true);
        VirtualizingPanel.SetVirtualizationMode(_listBox, VirtualizationMode.Recycling);

        SetRow(_listBox, 0);
        SetColumn(_listBox, 0);
        Children.Add(_listBox);

        SetRow(_button, 1);
        SetColumn(_button, 0);
        Children.Add(_button);

        Grid grid = new();
        SetRow(grid, 1);
        SetColumn(grid, 0);
        Children.Add(grid);

        SetRow(_textBlock, 0);
        SetColumn(_textBlock, 0);
        grid.Children.Add(_textBlock);

        SetRow(_progressBar, 1);
        SetColumn(_progressBar, 0);
        grid.Children.Add(_progressBar);

        SetZIndex(_progressBar, int.MinValue);
        SetZIndex(_textBlock, int.MaxValue);

        Loaded += OnLoaded;
        _button.Click += OnButtonClick;
        Application.Current.MainWindow.Closing += OnClosing;
    }

    void OnClosing(object sender, CancelEventArgs args)
    {
        args.Cancel = _task is { };
    }

    void Callback(int value, bool state) => Dispatcher.Invoke(() =>
    {
        string text = state ? "Installing..." : "Downloading...";

        if (value <= 0)
        {
            _progressBar.Value = 0;
            _textBlock.Text = $"{text}";
            _progressBar.IsIndeterminate = true;
            return;
        }

        _progressBar.Value = value;
        _progressBar.IsIndeterminate = false;
        _textBlock.Text = $"{text} {value}%";
    });

    async void OnButtonClick(object sender, RoutedEventArgs args)
    {
        try
        {
            _listBox.IsEnabled = false;
            _button.Visibility = Visibility.Hidden;

            _textBlock.Text = "Downloading...";
            _textBlock.Visibility = Visibility.Visible;

            _progressBar.Value = 0;
            _progressBar.IsIndeterminate = true;
            _progressBar.Visibility = Visibility.Visible;

            var item = (VersionItem)_listBox.SelectedItem;

            if (!Minecraft.IsInstalled)
            {
                await MainDialog.NotInstalled.ShowAsync();
                return;
            }

            if (!Minecraft.IsPackaged)
            {
                await MainDialog.UnpackagedInstall.ShowAsync();
                return;
            }

            if (_listBox.SelectedItem is null)
            {
                await MainDialog.SelectVersion.ShowAsync();
                return;
            }

            if (!await MainDialog.InstallVersion.ShowAsync())
                return;

            _listBox.ScrollIntoView(item);
            await (_task = item.InstallAsync(Callback));
        }
        finally
        {
            _task = null;

            _listBox.IsEnabled = true;
            _button.Visibility = Visibility.Visible;

            _textBlock.Text = "Downloading...";
            _textBlock.Visibility = Visibility.Collapsed;

            _progressBar.Value = 0;
            _progressBar.IsIndeterminate = false;
            _progressBar.Visibility = Visibility.Collapsed;
        }
    }

    async void OnLoaded(object sender, RoutedEventArgs args)
    {
        await Task.Run(async () =>
        {
            foreach (var item in await VersionRegistry.CreateAsync())
                Dispatcher.Invoke(() => _listBox.Items.Add(item));
        });

        _listBox.IsEnabled = true;
        _button.Visibility = Visibility.Visible;

        _textBlock.Text = "Preparing...";
        _textBlock.Visibility = Visibility.Collapsed;

        _progressBar.Value = 0;
        _progressBar.IsIndeterminate = false;
        _progressBar.Visibility = Visibility.Collapsed;
    }
}
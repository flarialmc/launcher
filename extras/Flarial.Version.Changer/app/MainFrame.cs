using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Game;
using Flarial.Launcher.Services.Versions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

sealed class MainFrame : XamlElement<Grid>
{
    readonly ListBox _listBox = new()
    {
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        IsEnabled = false
    };

    readonly Button _button = new()
    {
        Content = "Install",
        Opacity = 0,
        IsEnabled = false,
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
    };

    readonly ProgressBar _progressBar = new()
    {
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        IsIndeterminate = true
    };

    readonly SymbolIcon _symbolIcon = new()
    {
        Symbol = Symbol.Refresh,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center
    };

    async void Callback(int value, bool state) => await Dispatcher.InvokeAsync(() =>
    {
        _symbolIcon.Symbol = state ? Symbol.Upload : Symbol.Download;

        if (value <= 0)
        {
            _progressBar.Value = 0;
            _progressBar.IsIndeterminate = true;
            return;
        }

        _progressBar.Value = value;
        _progressBar.IsIndeterminate = false;
    });

    async void OnLoaded(object sender, RoutedEventArgs args)
    {
        await Task.Run(async () =>
        {
            foreach (var item in await VersionRegistry.CreateAsync())
                await Dispatcher.InvokeAsync(() => _listBox.Items.Add(item));
        });

        _listBox.IsEnabled = true;

        _button.Opacity = 100;
        _button.IsEnabled = true;

        _symbolIcon.Visibility = Visibility.Collapsed;
        _progressBar.Visibility = Visibility.Collapsed;
    }

    Task? _task;

    void OnClosing(object sender, CancelEventArgs args)
    {
        args.Cancel = _task is { };
    }

    async void OnButtonClick(object sender, RoutedEventArgs args)
    {
        try
        {
            _listBox.IsEnabled = false;

            _button.Opacity = 0;
            _button.IsEnabled = false;

            _symbolIcon.Symbol = Symbol.Download;
            _symbolIcon.Visibility = Visibility.Visible;

            _progressBar.Value = 0;
            _progressBar.IsIndeterminate = true;
            _progressBar.Visibility = Visibility.Visible;

            var item = (VersionItem)_listBox.SelectedItem;

            if (!Minecraft.IsInstalled)
            {
                await MainDialog.NotInstalled.ShowAsync(this);
                return;
            }

            if (!Minecraft.IsPackaged)
            {
                await MainDialog.UnpackagedInstall.ShowAsync(this);
                return;
            }

            if (_listBox.SelectedItem is null)
            {
                await MainDialog.SelectVersion.ShowAsync(this);
                return;
            }

            if (!await MainDialog.InstallVersion.ShowAsync(this))
                return;

            _listBox.ScrollIntoView(item);
            await (_task = item.InstallAsync(Callback));
        }
        finally
        {
            _task = null;

            _listBox.IsEnabled = true;

            _button.Opacity = 100;
            _button.IsEnabled = true;

            _symbolIcon.Symbol = Symbol.Download;
            _symbolIcon.Visibility = Visibility.Collapsed;

            _progressBar.Value = 0;
            _progressBar.IsIndeterminate = false;
            _progressBar.Visibility = Visibility.Collapsed;
        }
    }

    internal MainFrame() : base(new())
    {
        Element.RowSpacing = 12;
        Element.Margin = new(12);

        Element.RowDefinitions.Add(new());
        Element.RowDefinitions.Add(new() { Height = GridLength.Auto });

        Element.Loaded += OnLoaded;
        _button.Click += OnButtonClick;
        System.Windows.Application.Current.MainWindow.Closing += OnClosing;

        _listBox.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
        VirtualizingStackPanel.SetVirtualizationMode(_listBox, VirtualizationMode.Recycling);

        Grid.SetRow(_listBox, 0);
        Grid.SetColumn(_listBox, 0);

        Grid.SetRow(_button, 1);
        Grid.SetColumn(_button, 0);

        Grid grid = new()
        {
            ColumnSpacing = 12,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        Grid.SetRow(grid, 1);
        Grid.SetColumn(grid, 0);

        grid.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new());

        Grid.SetRow(_symbolIcon, 0);
        Grid.SetColumn(_symbolIcon, 0);

        Grid.SetRow(_progressBar, 0);
        Grid.SetColumn(_progressBar, 1);

        grid.Children.Add(_symbolIcon);
        grid.Children.Add(_progressBar);

        Element.Children.Add(_listBox);
        Element.Children.Add(_button);
        Element.Children.Add(grid);
    }
}
using System.ComponentModel;
using Flarial.Launcher.Controls;
using Flarial.Launcher.Interface;
using Flarial.Launcher.Management;
using Flarial.Launcher.Xaml;
using Flarial.Runtime.Game;
using Flarial.Runtime.Versions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Pages;

sealed class VersionsPage : Grid
{
    internal readonly ListBox _listBox = new()
    {
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch
    };

    internal readonly Button _button = new()
    {
        Content = "Install",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        IsEnabled = false
    };

    readonly XamlContent _content;
    readonly AppSettings _settings;
    readonly TextBlockProgressBar _progressBar = new();

    void OnVersionItemInstallAsync(int value, bool state) => Dispatcher.Invoke(() =>
    {
        string text = state ? "Installing..." : "Downloading...";
        _progressBar._progressBar.Value = value <= 0 ? 0 : value;
        _progressBar._textBlock.Text = value <= 0 ? $"{text}" : $"{text} {value}%";
    });

    async void OnButtonClick(object sender, RoutedEventArgs args)
    {
        try
        {
            _button.Opacity = 0;
            _button.IsEnabled = false;

            _progressBar._textBlock.Text = "Downloading...";
            _progressBar._textBlock.Visibility = Visibility.Visible;

            _progressBar._progressBar.Value = 0;
            _progressBar._progressBar.Visibility = Visibility.Visible;

            if (!Minecraft.IsInstalled)
            {
                await DialogRegistry.NotInstalled.ShowAsync();
                return;
            }

            if (!Minecraft.IsPackaged)
            {
                await DialogRegistry.UnpackagedInstall.ShowAsync();
                return;
            }

            if (!Minecraft.IsGamingServicesInstalled)
            {
                await DialogRegistry.GamingServicesMissing.ShowAsync();
                return;
            }

            if (_listBox.SelectedItem is null)
            {
                await DialogRegistry.SelectVersion.ShowAsync();
                return;
            }

            if (!await DialogRegistry.InstallVersion.ShowAsync())
                return;

            _item = (VersionItem)_listBox.SelectedItem;
            _listBox.ScrollIntoView(_item);

            await _item.InstallAsync(OnVersionItemInstallAsync);
        }
        finally
        {
            _item = null;

            _button.Opacity = 100;
            _button.IsEnabled = true;

            _progressBar._textBlock.Text = "Downloading...";
            _progressBar._textBlock.Visibility = Visibility.Collapsed;

            _progressBar._progressBar.Value = 0;
            _progressBar._progressBar.Visibility = Visibility.Collapsed;
        }
    }

    void OnNavigationViewContentChanged(DependencyObject sender, DependencyProperty args)
    {
        if (_item is null)
        {
            _listBox.SelectedItem = null;
            _listBox.ScrollIntoView(_listBox.Items[0]);
        }
    }

    void OnListBoxSelectionChanged(object sender, RoutedEventArgs args)
    {
        var listBox = (ListBox)sender; if (_item is { })
        {
            listBox.SelectedItem = _item;
            listBox.ScrollIntoView(_item);
        }
    }

    void OnWindowClosing(object sender, CancelEventArgs args)
    {
        if (_item is { })
        {
            args.Cancel = true;
            (~_content).SelectedItem = _content._versionsItem;
            (~_content).Content = _content._versionsItem.Tag;
        }
    }

    VersionItem? _item = null;

    internal VersionsPage(XamlContent content, AppSettings settings)
    {
        _content = content;
        _settings = settings;

        RowSpacing = 12;
        Margin = new(12);

        RowDefinitions.Add(new());
        RowDefinitions.Add(new() { Height = GridLength.Auto });

        SetRow(_listBox, 0);
        SetColumn(_listBox, 0);

        SetRow(_button, 1);
        SetColumn(_button, 0);

        SetRow(_progressBar, 1);
        SetColumn(_progressBar, 0);

        Children.Add(_listBox);
        Children.Add(_button);
        Children.Add(_progressBar);

        _button.Click += OnButtonClick;
        _listBox.SelectionChanged += OnListBoxSelectionChanged;

        _listBox.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
        VirtualizingStackPanel.SetVirtualizationMode(_listBox, VirtualizationMode.Recycling);

        System.Windows.Application.Current.MainWindow.Closing += OnWindowClosing;
        (~_content).RegisterPropertyChangedCallback(ContentControl.ContentProperty, OnNavigationViewContentChanged);
    }
}
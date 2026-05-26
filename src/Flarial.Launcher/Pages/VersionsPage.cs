using System.ComponentModel;
using Flarial.Launcher.Controls;
using Flarial.Launcher.Interface.Dialogs;
using Flarial.Launcher.Interface.Presentation;
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

    readonly AppContent _content;
    readonly AppSettings _settings;
    readonly TextBlockProgressBar _progressBar = new();

    void OnInstall(int percentage, bool installing) => Dispatcher.Invoke(() =>
    {
        string text = installing ? "Installing..." : "Downloading...";
        _progressBar._progressBar.Value = percentage <= 0 ? 0 : percentage;
        _progressBar._textBlock.Text = percentage <= 0 ? $"{text}" : $"{text} {percentage}%";
    });

    async void OnClick(object sender, RoutedEventArgs args)
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
                await NotInstalledDialog.ShowAsync();
                return;
            }

            if (!Minecraft.IsPackaged)
            {
                await UnpackagedInstallDialog.ShowAsync();
                return;
            }

            if (!Minecraft.IsGamingServicesInstalled)
            {
                await GamingServicesMissingDialog.ShowAsync();
                return;
            }

            if (_listBox.SelectedItem is null)
            {
                await SelectVersionDialog.ShowAsync();
                return;
            }

            if (!await InstallVersionDialog.ShowAsync())
                return;

            _item = (VersionItem)_listBox.SelectedItem;
            _listBox.ScrollIntoView(_item);

            await _item.InstallAsync(OnInstall);
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

    void OnContentChanged(DependencyObject sender, DependencyProperty args)
    {
        if (_item is null)
        {
            _listBox.SelectedItem = null;
            _listBox.ScrollIntoView(_listBox.Items[0]);
        }
    }

    void OnSelectionChanged(object sender, RoutedEventArgs args)
    {
        if (_item is { })
        {
            _listBox.SelectedItem = _item;
            _listBox.ScrollIntoView(_item);
        }
    }

    void OnWindowClosing(object sender, CancelEventArgs args)
    {
        if (_item is { })
        {
            args.Cancel = true;
            (~_content).Content = _content._versionsItem.Tag;
            (~_content).SelectedItem = _content._versionsItem;
        }
    }

    VersionItem? _item = null;

    internal VersionsPage(AppContent content, AppSettings settings)
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

        _button.Click += OnClick;
        _listBox.SelectionChanged += OnSelectionChanged;

        _listBox.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
        VirtualizingStackPanel.SetVirtualizationMode(_listBox, VirtualizationMode.Recycling);

        System.Windows.Application.Current.MainWindow.Closing += OnWindowClosing;
        (~_content).RegisterPropertyChangedCallback(ContentControl.ContentProperty, OnContentChanged);
    }
}
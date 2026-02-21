using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Flarial.Launcher.Controls;
using Flarial.Launcher.Interface;
using Flarial.Launcher.Runtime.Game;
using Flarial.Launcher.Runtime.Versions;
using Flarial.Launcher.Xaml;
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

    readonly MainNavigationView _view;
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

            if (Minecraft.UsingGameDevelopmentKit && !Minecraft.IsGamingServicesInstalled)
            {
                await MainDialog.GamingServicesMissing.ShowAsync(this);
                return;
            }

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
            (!_view).SelectedItem = _view._versionsItem;
            (!_view).Content = _view._versionsItem.Tag;
        }
    }

    VersionItem? _item = null;

    internal VersionsPage(MainNavigationView view)
    {
        _view = view;

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
        (!view).RegisterPropertyChangedCallback(ContentControl.ContentProperty, OnNavigationViewContentChanged);
    }
}
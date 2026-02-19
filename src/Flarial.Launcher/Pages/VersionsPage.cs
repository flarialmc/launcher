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

sealed class VersionsPage : XamlElement<Grid>
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

    void OnVersionItemInstallAsync(int value, bool state) => @this.Dispatcher.Invoke(() =>
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
                await MainDialog.NotInstalled.ShowAsync(@this);
                return;
            }

            if (!Minecraft.IsPackaged)
            {
                await MainDialog.UnpackagedInstall.ShowAsync(@this);
                return;
            }

            if (_listBox.SelectedItem is null)
            {
                await MainDialog.SelectVersion.ShowAsync(@this);
                return;
            }

            if (!await MainDialog.InstallVersion.ShowAsync(@this))
                return;

            if (Minecraft.UsingGameDevelopmentKit && !Minecraft.IsGamingServicesInstalled)
            {
                await MainDialog.GamingServicesMissing.ShowAsync(@this);
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
            _view.@this.Content = @this;
            _view.@this.SelectedItem = _view._versionsItem;
        }
    }

    VersionItem? _item = null;

    internal VersionsPage(MainNavigationView view) : base(new())
    {
        _view = view;

        @this.RowSpacing = 12;
        @this.Margin = new(12);

        @this.RowDefinitions.Add(new());
        @this.RowDefinitions.Add(new() { Height = GridLength.Auto });

        Grid.SetRow(_listBox, 0);
        Grid.SetColumn(_listBox, 0);

        Grid.SetRow(_button, 1);
        Grid.SetColumn(_button, 0);

        Grid.SetRow(_progressBar.@this, 1);
        Grid.SetColumn(_progressBar.@this, 0);

        @this.Children.Add(_listBox);
        @this.Children.Add(_button);
        @this.Children.Add(_progressBar.@this);

        _button.Click += OnButtonClick;
        _listBox.SelectionChanged += OnListBoxSelectionChanged;

        _listBox.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
        VirtualizingStackPanel.SetVirtualizationMode(_listBox, VirtualizationMode.Recycling);

        System.Windows.Application.Current.MainWindow.Closing += OnWindowClosing;
        view.@this.RegisterPropertyChangedCallback(ContentControl.ContentProperty, OnNavigationViewContentChanged);
    }
}
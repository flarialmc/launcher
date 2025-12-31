using System.Windows;
using System.Windows.Controls;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Management.Versions;
using Flarial.Launcher.UI.Controls;
using static Windows.ApplicationModel.Store.Preview.InstallControl.AppInstallState;
using static ModernWpf.Controls.Symbol;
using System.Windows.Threading;
using System.ComponentModel;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using static Flarial.Launcher.Interface.MessageDialogContent;

namespace Flarial.Launcher.Interface.Pages;

sealed class VersionsPage : Grid
{
    internal readonly ListBox _listBox = new()
    {
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new(0, 0, 0, 12)
    };

    readonly InstallProgressControl _control = new()
    {
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch
    };

    InstallRequest? _request = null;

    internal VersionsPage()
    {
        Margin = new(12);

        RowDefinitions.Add(new());
        RowDefinitions.Add(new() { Height = GridLength.Auto });

        SetRow(_listBox, 0);
        SetColumn(_listBox, 0);
        Children.Add(_listBox);

        SetRow(_control, 1);
        SetColumn(_control, 0);
        Children.Add(_control);

        _control._button.Click += OnInstallButtonClick;
        Application.Current.MainWindow.Closing += OnWindowClosing;
    }

    void OnWindowClosing(object sender, CancelEventArgs args) => args.Cancel = _request?.State is Installing;

    void InvokeOnInstallProgress(AppInstallState state, int value) => Dispatcher.Invoke(DispatcherPriority.Send, OnInstallProgress, state, value);

    void OnInstallProgress(AppInstallState state, int value)
    {
        switch (state)
        {
            case Installing: _control._icon.Symbol = Upload; break;
            case Downloading: _control._icon.Symbol = Download; break;
        }

        if (value <= 0)
        {
            _control._progressBar.Value = 0;
            _control._progressBar.IsIndeterminate = true;
        }

        if (_control._progressBar.Value != value)
        {
            _control._progressBar.Value = value;
            _control._progressBar.IsIndeterminate = false;
        }
    }

    async void OnInstallButtonClick(object sender, RoutedEventArgs args)
    {
        try
        {
            IsEnabled = false;

            if (!Minecraft.IsInstalled)
            {
                await MessageDialog.ShowAsync(MessageDialogContent._notInstalled);
                return;
            }

            if (!Minecraft.IsPackaged)
            {
                await MessageDialog.ShowAsync(MessageDialogContent._unpackagedInstallationDetected);
                return;
            }

            _control._icon.Symbol = Download;
            _control._icon.Visibility = Visibility.Visible;

            _control._progressBar.Visibility = Visibility.Visible;
            _control._progressBar.IsIndeterminate = true;

            _control._button.Visibility = Visibility.Hidden;

            try
            {
                var item = (ListBoxItem)_listBox.SelectedItem;
                var entry = (VersionEntry)item.Tag;

                _request = await entry.InstallAsync(InvokeOnInstallProgress);
                await _request;
            }
            finally { _request = null; }

        }
        finally
        {
            _control._progressBar.Value = 0;
            _control._progressBar.IsIndeterminate = false;
            _control._progressBar.Visibility = Visibility.Hidden;

            _control._icon.Symbol = Download;
            _control._icon.Visibility = Visibility.Collapsed;

            _control._button.Visibility = Visibility.Visible;
            IsEnabled = true;
        }
    }
}
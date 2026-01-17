using System.Windows;
using System.Windows.Controls;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Management.Versions;
using static Windows.ApplicationModel.Store.Preview.InstallControl.AppInstallState;
using static ModernWpf.Controls.Symbol;
using System.Windows.Threading;
using static Flarial.Launcher.Interface.MessageDialog;
using Flarial.Launcher.Interface.Controls;
using System.Threading.Tasks;
using static System.ComponentModel.DependencyPropertyDescriptor;
using System;
using System.ComponentModel;
using Windows.ApplicationModel.Store.Preview.InstallControl;

namespace Flarial.Launcher.Interface.Pages;

sealed class VersionsPage : Grid
{
    readonly RootPage _rootPage;

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

    void OnContentChanged(object sender, EventArgs args)
    {
        if (_listBox.IsEnabled)
            _listBox.SelectedIndex = -1;
    }

    void OnClosing(object sender, CancelEventArgs args)
    {
        if (_task is { })
        {
            args.Cancel = true;
            _rootPage.Content = this;
            _rootPage._versionsPageItem.IsSelected = true;
        }
    }

    void OnVersionEntryInstallAsync(AppInstallState state, int value)
    {
        if (!CheckAccess())
        {
            Dispatcher.Invoke(OnVersionEntryInstallAsync, state, value);
            return;
        }

        switch (state)
        {
            case Installing:
                _control._icon.Symbol = Upload;
                break;

            case Downloading:
                _control._icon.Symbol = Download;
                break;
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

    async void OnButtonClick(object sender, EventArgs args)
    {
        try
        {
            IsEnabled = false;

            if (!Minecraft.IsInstalled)
            {
                await _notInstalled.ShowAsync();
                return;
            }

            if (!Minecraft.IsPackaged)
            {
                await _unpackagedInstallation.ShowAsync();
                return;
            }

            if (_listBox.SelectedItem is null)
            {
                await _selectVersion.ShowAsync();
                return;
            }

            if (!await _installVersion.ShowAsync())
                return;

            _control._icon.Symbol = Download;
            _control._icon.Visibility = Visibility.Visible;

            _control._progressBar.IsIndeterminate = true;
            _control._progressBar.Visibility = Visibility.Visible;

            _control._button.Visibility = Visibility.Hidden;

            try
            {
                var item = (ListBoxItem)_listBox.SelectedItem;
                var entry = (VersionEntry)item.Tag;

                _task = entry.InstallAsync(OnVersionEntryInstallAsync);
                await _task;
            }
            finally { _task = null; }
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

    Task? _task = null;

    internal VersionsPage(RootPage rootPage)
    {
        _rootPage = rootPage;

        Margin = new(12);

        RowDefinitions.Add(new());
        RowDefinitions.Add(new() { Height = GridLength.Auto });

        SetRow(_listBox, 0);
        SetColumn(_listBox, 0);
        Children.Add(_listBox);

        SetRow(_control, 1);
        SetColumn(_control, 0);
        Children.Add(_control);

        Application.Current.MainWindow.Closing += OnClosing;
        FromProperty(ContentControl.ContentProperty, typeof(ContentControl)).AddValueChanged(rootPage, OnContentChanged);

        _control._button.Click += OnButtonClick;
    }
}
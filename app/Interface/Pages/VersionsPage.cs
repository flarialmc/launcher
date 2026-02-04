using System.Windows;
using System.Windows.Controls;
using Flarial.Launcher.Services.Game;
using Flarial.Launcher.Services.Versions;
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
using ModernWpf.Controls.Primitives;
using System.Runtime.CompilerServices;

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
        if (_listBox.IsEnabled && _listBox.HasItems)
        {
            _listBox.SelectedIndex = -1;
            _listBox.ScrollIntoView(_listBox.Items[0]);
        }
    }

    void OnClosing(object sender, CancelEventArgs args)
    {
        if (_task is { })
        {
            args.Cancel = true;
            Dispatcher.Invoke(() =>
            {
                _rootPage.Content = this;
                _rootPage._versionsPageItem.IsSelected = true;
            }, DispatcherPriority.Background);
        }
    }

    void OnVersionEntryInstallAsync(int value, bool installing) => Dispatcher.Invoke(() =>
    {
        _control._icon.Symbol = installing ? Upload : Download;

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
    }, DispatcherPriority.Background);

    void SetVisiblity(bool visible) => Dispatcher.Invoke(() =>
    {
        _listBox.IsEnabled = visible;
        _control._button.Visibility = visible ? Visibility.Visible : Visibility.Hidden;

        _control._progressBar.Value = 0;
        _control._progressBar.IsIndeterminate = visible ? false : true;
        _control._progressBar.Visibility = visible ? Visibility.Hidden : Visibility.Visible;

        _control._icon.Symbol = Download;
        _control._icon.Visibility = visible ? Visibility.Collapsed : Visibility.Visible;
    }, DispatcherPriority.Background);


    async void OnButtonClick(object sender, EventArgs args)
    {
        if (!Minecraft.Installed)
        {
            await _notInstalled.ShowAsync();
            return;
        }

        if (!Minecraft.Packaged)
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

        SetVisiblity(false); try
        {
            VersionItem entry; unsafe
            {
                var @object = _listBox.SelectedItem;
                var item = *(ListBoxItem*)&@object;

                var tag = item.Tag;
                entry = *(VersionItem*)&tag;
            }
            await (_task = entry.InstallAsync(OnVersionEntryInstallAsync));
        }
        finally { _task = null; SetVisiblity(true); }
    }

    Task? _task = null;

    internal VersionsPage(RootPage rootPage)
    {
        _rootPage = rootPage;
        ScrollViewerHelper.SetAutoHideScrollBars(_listBox, true);

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
using System.Windows;
using System.Windows.Controls;
using Flarial.Launcher.Services.Game;
using Flarial.Launcher.Services.Versions;
using System.Windows.Threading;
using Flarial.Launcher.Interface.Controls;
using System.Threading.Tasks;
using System;
using System.ComponentModel;
using ModernWpf.Controls.Primitives;
using ModernWpf.Controls;
using System.Diagnostics;

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
            _rootPage.Content = this;
            _rootPage._versionsPageItem.IsSelected = true;
        }
    }

    void InvokeVersionEntryInstallAsync(int value, bool state) => Dispatcher.Invoke(() =>
    {
        _control._icon.Symbol = state switch
        {
            true => Symbol.Upload,
            false => Symbol.Download
        };

        if (value <= 0)
        {
            _control._progressBar.Value = 0;
            _control._progressBar.IsIndeterminate = true;
            return;
        }

        if (_control._progressBar.Value != value)
        {
            _control._progressBar.Value = value;
            _control._progressBar.IsIndeterminate = false;
        }
    });

    void SetVisibility(bool visible)
    {
        _listBox.IsEnabled = visible;

        _control._progressBar.Value = 0;
        _control._progressBar.IsIndeterminate = !visible;

        _control._icon.Symbol = Symbol.Download;
        _control._button.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
        _control._icon.Visibility = visible ? Visibility.Collapsed : Visibility.Visible;
        _control._progressBar.Visibility = visible ? Visibility.Collapsed : Visibility.Visible;
    }

    async void OnButtonClick(object sender, EventArgs args)
    {
        try
        {
            SetVisibility(false);
            var item = (VersionItem)_listBox.SelectedItem;

            if (!Minecraft.IsInstalled)
            {
                await AppDialog.NotInstalled.ShowAsync();
                return;
            }

            if (!Minecraft.IsPackaged)
            {
                await AppDialog.UnpackagedInstall.ShowAsync();
                return;
            }

            if (item.IsGameDevelopmentKit || Minecraft.UsingGameDevelopmentKit)
            {
                if (!Minecraft.IsGamingServicesInstalled)
                {
                    await AppDialog.GamingServicesMissing.ShowAsync();
                    return;
                }
            }

            if (_listBox.SelectedItem is null)
            {
                await AppDialog.SelectVersion.ShowAsync();
                return;
            }

            if (!await AppDialog.InstallVersion.ShowAsync())
                return;

            _listBox.ScrollIntoView(item);
            await (_task = item.InstallAsync(InvokeVersionEntryInstallAsync));
        }
        finally
        {
            _task = null;
            SetVisibility(true);
        }
    }

    Task? _task = null;

    internal VersionsPage(RootPage rootPage)
    {
        _rootPage = rootPage;

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

        SetRow(_control, 1);
        SetColumn(_control, 0);
        Children.Add(_control);

        Application.Current.MainWindow.Closing += OnClosing;
        DependencyPropertyDescriptor.FromProperty(ContentControl.ContentProperty, typeof(ContentControl)).AddValueChanged(rootPage, OnContentChanged);

        _control._button.Click += OnButtonClick;
    }
}
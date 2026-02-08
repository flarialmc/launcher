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
using System.Windows.Interop;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using ModernWpf.Controls;
using Windows.Management.Deployment;
using Flarial.Launcher.Management;
using System.Linq;

namespace Flarial.Launcher.Interface.Pages;

sealed class VersionsPage : Grid
{
    readonly RootPage _rootPage;
    static readonly PackageManager s_packageManager = new();

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

            var listBoxItem = (ListBoxItem)_listBox.SelectedItem;
            var versionItem = (VersionItem)listBoxItem.Tag;

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

            if (versionItem.IsGameDevelopmentKit || Minecraft.UsingGameDevelopmentKit)
            {
                if (!Minecraft.IsGamingServicesInstalled)
                {
                    await MainDialog.GamingServicesMissing.ShowAsync();
                    return;
                }
            }

            if (_listBox.SelectedItem is null)
            {
                await MainDialog.SelectVersion.ShowAsync();
                return;
            }

            if (!await MainDialog.InstallVersion.ShowAsync())
                return;

            _listBox.ScrollIntoView(listBoxItem);
            await (_task = versionItem.InstallAsync(InvokeVersionEntryInstallAsync));
        }
        finally
        {
            _task = null;
            SetVisibility(true);
        }
    }

    //    void ShellExecute(string lpFile) => PInvoke.ShellExecute(_helper.EnsureHandle(), null!, lpFile, null!, null!, PInvoke.SW_NORMAL);

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
        DependencyPropertyDescriptor.FromProperty(ContentControl.ContentProperty, typeof(ContentControl)).AddValueChanged(rootPage, OnContentChanged);

        _control._button.Click += OnButtonClick;
    }
}
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Flarial.Launcher.Interface.Pages;
using Flarial.Launcher.Management;
using Flarial.Launcher.Services.Management;
using Flarial.Launcher.Services.Management.Versions;
using Flarial.Launcher.Services.Networking;
using static Flarial.Launcher.Interface.MessageDialogContent;
using ModernWpf.Controls;
using System;
using System.Windows.Interop;

namespace Flarial.Launcher.Interface;

sealed class MainWindowContent : NavigationView
{
    HomePage? _homePage = null;
    readonly VersionsPage _versionsPage;
    readonly SettingsPage _settingsPage;

    internal readonly NavigationViewItem _homePageItem = new()
    {
        Icon = new SymbolIcon(Symbol.Home),
        Content = "Home",
        Tag = Symbol.Home,
        IsSelected = true
    };

    internal readonly NavigationViewItem _versionsPageItem = new()
    {
        Icon = new SymbolIcon(Symbol.BrowsePhotos),
        Content = "Versions",
        Tag = Symbol.BrowsePhotos,
    };

    internal readonly NavigationViewItem _settingsPageItem = new()
    {
        Icon = new SymbolIcon(Symbol.Setting),
        Content = "Settings",
        Tag = Symbol.Setting,
    };

    readonly ModernWpf.Controls.ProgressBar _progressBar = new()
    {
        Width = ApplicationManifest.Icon.Width ,
        Foreground = new SolidColorBrush(Colors.White),
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        IsIndeterminate = true
    };

    internal MainWindowContent(Configuration configuration, WindowInteropHelper helper)
    {
        IsPaneVisible = false;
        IsPaneOpen = false;
        IsSettingsVisible = false;

        PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
        IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;

        Content = _progressBar;

        MenuItems.Add(_homePageItem);
        MenuItems.Add(_versionsPageItem);
        FooterMenuItems.Add(_settingsPageItem);

        ItemInvoked += (sender, args) =>
        {
            switch ((Symbol)args.InvokedItemContainer.Tag)
            {
                case Symbol.Home: Content = _homePage; break;
                case Symbol.BrowsePhotos: Content = _versionsPage; break;
                case Symbol.Setting: Content = _settingsPage; break;
            }
        };

        _versionsPage = new(this);
        _settingsPage = new(configuration, helper);

        Dispatcher.Invoke(async () =>
        {
            var sponsorship = Sponsorship.GetAsync(helper);

            if (!await HttpService.IsAvailableAsync() &&
                !await MessageDialog.ShowAsync(_connectionFailure))
                Application.Current.Shutdown();

            if (await LauncherUpdater.CheckAsync() &&
                await MessageDialog.ShowAsync(_launcherUpdateAvailable))
            {
                await LauncherUpdater.DownloadAsync((_) => Dispatcher.Invoke(() =>
                {
                    if (_progressBar.Value == _) return;
                    _progressBar.Value = _;
                    _progressBar.IsIndeterminate = false;
                }));
                return;
            }

            var catalog = VersionEntries.CreateAsync();
            _homePage = new(configuration, await catalog, sponsorship);

            foreach (var entry in await catalog)
            {
                await Dispatcher.Yield(); if (entry.Value is null) continue;
                _versionsPage._listBox.Items.Add(new ListBoxItem { Content = entry.Key, Tag = entry.Value });
            }
            _versionsPage._listBox.SelectedIndex = 0;

            Content = _homePage;
            _progressBar.IsIndeterminate = false;

            IsPaneVisible = true;
        }, DispatcherPriority.Send);
    }
}
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
    readonly SettingsPage _settingsPage;

    internal readonly NavigationViewItem _homePageItem = new()
    {
        Icon = new SymbolIcon(Symbol.Home),
        Content = "Home",
        IsSelected = true
    };

    internal readonly NavigationViewItem _versionsPageItem = new()
    {
        Icon = new SymbolIcon(Symbol.AllApps),
        Content = "Versions"
    };

    readonly ModernWpf.Controls.ProgressBar _progressBar = new()
    {
        Width = ApplicationManifest.Icon.Width,
        Foreground = new SolidColorBrush(Colors.White),
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        IsIndeterminate = true
    };

    internal MainWindowContent(Configuration configuration, WindowInteropHelper helper)
    {
        IsPaneOpen = false;
        IsPaneVisible = false;

        PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
        IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;

        Content = _progressBar;

        MenuItems.Add(_homePageItem);
        MenuItems.Add(_versionsPageItem);

        ItemInvoked += (sender, args) =>
        {
            if (args.IsSettingsInvoked) Content = _settingsPage; 
            else Content = args.InvokedItemContainer.Tag;
        };

        _settingsPage = new(configuration, helper);
        _versionsPageItem.Tag = new VersionsPage(this);

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

            var catalog = await VersionEntries.CreateAsync();
            _homePageItem.Tag = new HomePage(configuration, catalog, sponsorship);

            var versionsPage = (VersionsPage)_versionsPageItem.Tag;

            foreach (var entry in catalog)
            {
                await Dispatcher.Yield(); if (entry.Value is null) continue;
                versionsPage._listBox.Items.Add(new ListBoxItem { Tag = entry.Value, Content = entry.Key });
            }

            versionsPage._listBox.SelectedIndex = 0;

            Content = _homePageItem.Tag;
            _progressBar.IsIndeterminate = false;

            IsPaneVisible = true;
        }, DispatcherPriority.Send);
    }
}
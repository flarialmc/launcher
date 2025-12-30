using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Flarial.Launcher.App;
using Flarial.Launcher.Services.Management;
using Flarial.Launcher.Services.Management.Versions;
using Flarial.Launcher.Services.Networking;
using Flarial.Launcher.UI.Pages;
using ModernWpf.Controls;

namespace Flarial.Launcher.UI;

sealed class MainWindowContent : NavigationView
{
    HomePage? _homePage = null;
    readonly VersionsPage? _versionsPage = null;
    readonly SettingsPage _settingsPage;

    internal MainWindowContent(Configuration configuration)
    {
        _settingsPage = new(configuration);
        _versionsPage = new();

        IsEnabled = false;
        IsPaneOpen = false;
        IsSettingsVisible = false;

        PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
        IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;

        Content = new ModernWpf.Controls.ProgressBar()
        {
            Width = ApplicationManifest.Icon.Width * 2,
            Foreground = new SolidColorBrush(Colors.White),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            IsIndeterminate = true
        };

        MenuItems.Add(new NavigationViewItem
        {
            Icon = new SymbolIcon(Symbol.Home),
            Content = "Home",
            Tag = Symbol.Home,
            IsSelected = true
        });

        MenuItems.Add(new NavigationViewItem
        {
            Icon = new SymbolIcon(Symbol.BrowsePhotos),
            Content = "Versions",
            Tag = Symbol.BrowsePhotos,
        });

        FooterMenuItems.Add(new NavigationViewItem
        {
            Icon = new SymbolIcon(Symbol.Setting),
            Content = "Settings",
            Tag = Symbol.Setting,
        });

        ItemInvoked += (sender, args) =>
        {
            switch ((Symbol)args.InvokedItemContainer.Tag)
            {
                case Symbol.Home: Content = _homePage; break;
                case Symbol.BrowsePhotos: Content = _versionsPage; break;
                case Symbol.Setting: Content = _settingsPage; break;
            }
        };

        Dispatcher.Invoke(async () =>
        {
            if (!await HttpService.IsAvailableAsync() &&
                !await MessageDialog.ShowAsync(MessageDialogContent._connectionFailure))
                Application.Current.Shutdown();

            if (await LauncherUpdater.CheckAsync() &&
                await MessageDialog.ShowAsync(MessageDialogContent._launcherUpdate))
            {
                await LauncherUpdater.DownloadAsync(delegate { });
                return;
            }

            var sponsorship = Sponsorship.GetAsync();
            var catalog = VersionCatalog.CreateAsync();
            _homePage = new(configuration, await catalog, sponsorship);

            foreach (var entry in await catalog)
            {
                await Dispatcher.Yield(); if (entry.Value is null) continue;
                _versionsPage._listBox.Items.Add(new ListBoxItem { Content = entry.Key, Tag = entry.Value });
            }

            _versionsPage._listBox.SelectedIndex = 0;
            Content = _homePage; IsEnabled = true;
        }, DispatcherPriority.Send);
    }
}
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Flarial.Launcher.App;
using Flarial.Launcher.Services.Client;
using Flarial.Launcher.Services.Management;
using Flarial.Launcher.Services.Management.Versions;
using Flarial.Launcher.Services.Networking;
using Flarial.Launcher.UI.Pages;
using ModernWpf.Controls;

namespace Flarial.Launcher.UI;

sealed class MainWindowContent : NavigationView
{
    HomePage? _homePage = null;
    VersionsPage? _versionsPage = null;
    readonly SettingsPage _settingsPage;

    internal MainWindowContent(Configuration configuration)
    {
        _settingsPage = new(configuration);

        IsEnabled = false;
        IsPaneOpen = false;
        IsSettingsVisible = false;

        PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
        IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;

        Content = new ProgressRing()
        {
            Width = 50,
            Height = 50,
            IsActive = true,
            Foreground = new SolidColorBrush(Colors.White)
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
            Icon = new SymbolIcon(Symbol.AllApps),
            Content = "Versions",
            Tag = Symbol.AllApps,
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
                case Symbol.AllApps: Content = _versionsPage; break;
                case Symbol.Setting: Content = _settingsPage; break;
            }
        };

        Application.Current.MainWindow.ContentRendered += async (_, _) =>
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

            var task1 = Sponsorship.GetImageAsync();
            var task2 = VersionCatalog.GetAsync();
            await Task.WhenAll(task1, task2);

            var banner = await task1;
            var catalog = await task2;

            _versionsPage = new(catalog);
            _homePage = new(configuration, catalog, banner);

            foreach (var version in catalog.InstallableVersions)
            {
                _versionsPage._listBox.Items.Add(version);
                await System.Windows.Threading.Dispatcher.Yield();
            }

            _versionsPage._listBox.SelectedIndex = 0;
            Content = _homePage; IsEnabled = true;
        };
    }
}
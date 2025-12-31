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

namespace Flarial.Launcher.Interface;

sealed class MainWindowContent : NavigationView
{
    HomePage? _homePage = null;
    readonly VersionsPage _versionsPage = new();
    readonly SettingsPage _settingsPage;

    internal MainWindowContent(Configuration configuration)
    {
        _settingsPage = new(configuration);

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
                case Symbol.Home:
                    Content = _homePage;
                    break;

                case Symbol.BrowsePhotos:
                    Content = _versionsPage;
                    break;

                case Symbol.Setting:
                    Content = _settingsPage;
                    break;
            }
        };

        Dispatcher.Invoke(DispatcherPriority.Send, InitializeAsync, configuration);
    }

    async void InitializeAsync(Configuration configuration)
    {
        if (!await HttpService.IsAvailableAsync() &&
            !await MessageDialog.ShowAsync(_connectionFailure))
            Application.Current.Shutdown();

        if (await LauncherUpdater.CheckAsync() &&
            await MessageDialog.ShowAsync(_launcherUpdateAvailable))
        {
            await LauncherUpdater.DownloadAsync(delegate { });
            return;
        }

        var sponsorship = Sponsorship.GetAsync();
        var catalog = VersionEntries.CreateAsync();
        _homePage = new(configuration, await catalog, sponsorship);

        foreach (var entry in await catalog)
        {
            await Dispatcher.Yield();
            if (entry.Value is null) continue;

            _versionsPage._listBox.Items.Add(new ListBoxItem
            {
                Content = entry.Key,
                Tag = entry.Value
            });
        }
        _versionsPage._listBox.SelectedIndex = 0;

        Content = _homePage;
        IsEnabled = true;
    }
}
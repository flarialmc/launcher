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

namespace Flarial.Launcher.Interface.Pages;

sealed class RootPage : NavigationView
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


    internal RootPage(Configuration configuration, WindowInteropHelper helper)
    {
        IsPaneOpen = false;
        IsPaneVisible = false;

        PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
        IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;

        ModernWpf.Controls.ProgressBar progressBar = new()
        {
            Width = ApplicationManifest.Icon.Width,
            Foreground = new SolidColorBrush(Colors.White),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            IsIndeterminate = true
        };

        Content = progressBar;

        MenuItems.Add(_homePageItem);
        MenuItems.Add(_versionsPageItem);

        _settingsPage = new(configuration, helper);
        _versionsPageItem.Tag = new VersionsPage(this);

        ItemInvoked += (sender, args) => Content = args.IsSettingsInvoked ? _settingsPage : args.InvokedItemContainer.Tag;

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
                    if (progressBar.Value == _)
                        return;

                    progressBar.Value = _;
                    progressBar.IsIndeterminate = false;
                }, DispatcherPriority.Send));

                progressBar.IsIndeterminate = true;
                return;
            }

            var catalog = await VersionEntries.CreateAsync();

            foreach (var entry in catalog)
            {
                await Dispatcher.Yield();

                if (entry.Value is null)
                    continue;

                ((VersionsPage)_versionsPageItem.Tag)._listBox.Items.Add(new ListBoxItem
                {
                    Tag = entry.Value,
                    Content = entry.Key,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                });
            }

            Content = _homePageItem.Tag = new HomePage(configuration, catalog, sponsorship);

            IsPaneVisible = true;
        }, DispatcherPriority.Send);
    }
}
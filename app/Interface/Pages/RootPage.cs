using Flarial.Launcher.Management;
using ModernWpf.Controls;
using System.Windows.Interop;

namespace Flarial.Launcher.Interface.Pages;

sealed class RootPage : NavigationView
{
    internal readonly NavigationViewItem _homePageItem = new()
    {
        Icon = new SymbolIcon(Symbol.Home),
        Content = "Home",
        IsSelected = true
    };

    internal readonly NavigationViewItem _versionsPageItem = new()
    {
        IsEnabled = false,
        Icon = new SymbolIcon(Symbol.AllApps),
        Content = "Versions"
    };

    internal readonly NavigationViewItem _settingsPageItem = new()
    {
        Icon = new SymbolIcon(Symbol.Setting),
        Content = "Settings"
    };

    internal RootPage(Configuration configuration)
    {
        IsSettingsVisible = false;

        PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
        IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;

        MenuItems.Add(_homePageItem);
        MenuItems.Add(_versionsPageItem);
        FooterMenuItems.Add(_settingsPageItem);

        _settingsPageItem.Tag = new SettingsPage(configuration);
        ItemInvoked += (sender, args) => Content = args.InvokedItemContainer.Tag;
    }
}
using Flarial.Launcher.Management;
using ModernWpf.Controls;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

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

    internal readonly NavigationViewItem _settingsPageItem = new()
    {
        Icon = new SymbolIcon(Symbol.Setting),
        Content = "Settings"
    };

    internal RootPage(Configuration configuration, WindowInteropHelper helper)
    {
        Content = new ProgressBar()
        {
            IsIndeterminate = true,
            Width = ApplicationManifest.Icon.Width,
            Foreground = new SolidColorBrush(Colors.White),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
        IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
        IsSettingsVisible = IsEnabled = IsPaneOpen = IsPaneVisible = false;

        MenuItems.Add(_homePageItem);
        MenuItems.Add(_versionsPageItem);
        FooterMenuItems.Add(_settingsPageItem);

        _settingsPageItem.Tag = _settingsPage = new(configuration, helper);
        ItemInvoked += (sender, args) => Content = args.InvokedItemContainer.Tag;
    }
}
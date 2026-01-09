using Flarial.Launcher.Management;
using ModernWpf.Controls;
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

    internal RootPage(Configuration configuration, WindowInteropHelper helper)
    {
        Content = new ProgressBar()
        {
            Width = ApplicationManifest.Icon.Width,
            Foreground = new SolidColorBrush(Colors.White),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            IsIndeterminate = true
        };

        _settingsPage = new(configuration, helper);

        IsEnabled = false;
        IsPaneOpen = false;
        IsPaneVisible = false;

        PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
        IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;

        MenuItems.Add(_homePageItem);
        MenuItems.Add(_versionsPageItem);

        ItemInvoked += (sender, args) => Content = args.IsSettingsInvoked ? _settingsPage : args.InvokedItemContainer.Tag;
    }
}
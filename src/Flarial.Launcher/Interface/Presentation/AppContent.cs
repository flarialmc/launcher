using System;
using System.Threading.Tasks;
using Flarial.Launcher.Interface.Dialogs;
using Flarial.Launcher.Management;
using Flarial.Launcher.Pages;
using Flarial.Launcher.Xaml;
using Flarial.Runtime.Core;
using Flarial.Runtime.Game;
using Flarial.Runtime.Versions;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Interface.Presentation;

sealed class AppContent : XamlElement<NavigationView>
{
    internal readonly NavigationViewItem _homeItem = new()
    {
        Icon = new SymbolIcon(Symbol.Home),
        Content = "Home"
    };

    internal readonly NavigationViewItem _versionsItem = new()
    {
        Icon = new SymbolIcon(Symbol.AllApps),
        Content = "Versions"
    };

    readonly HomePage _homePage;
    readonly VersionsPage _versionsPage;
    readonly SettingsPage _settingsPage;

    readonly AppSettings _settings;

    internal AppContent(AppSettings settings) : base(new())
    {
        _settings = settings;

        _settingsPage = new(settings);
        _homePage = new(this, settings);
        _versionsPage = new(this, settings);

        _homeItem.Tag = _homePage;
        _versionsItem.Tag = _versionsPage;

        (~this).IsPaneOpen = false;
        (~this).UseLayoutRounding = true;
        (~this).PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
        (~this).IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;

        (~this).MenuItems.Add(_homeItem);
        (~this).MenuItems.Add(_versionsItem);

        (~this).Loaded += OnLoaded;
        (~this).ItemInvoked += OnItemInvoked;

        (~this).Content = _homePage;
        (~this).SelectedItem = _homeItem;

        Minecraft.PackageStatusChanged += OnPackageStatusChanged;
    }

    static void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        var container = args.InvokedItemContainer;
        sender.Content = container.Tag;
    }

    void OnFlarialLauncherDownloadAsync(int value) => (~this).Dispatcher.Invoke(() =>
    {
        _homePage._button.Content = $"Updating... {value}%";
    });

    void OnPackageStatusChanged()
    {
        if (!(~this).Dispatcher.HasThreadAccess)
        {
            (~this).Dispatcher.Invoke(OnPackageStatusChanged);
            return;
        }

        if (!Minecraft.IsInstalled)
        {
            _homePage._leftText.Text = "⚪ 0.0.0";
            return;
        }

        var registry = (VersionRegistry)(~this).Tag;
        var text = $"{(registry.IsSupported ? "🟢" : "🔴")} {VersionRegistry.InstalledVersion}";

        _homePage._leftText.Text = text;
    }

    async void OnLoaded(object sender, RoutedEventArgs args)
    {
        (~this).Loaded -= OnLoaded;
        AppDialog.Current.XamlRoot = (~this).XamlRoot;

        var settingsItem = (NavigationViewItem)(~this).SettingsItem;
        settingsItem.Tag = _settingsPage;

        if (!await FlarialLauncher.VerifyConnectionAsync())
        {
            await ConnectionFailureDialog.ShowAsync();
            System.Windows.Application.Current.Shutdown();
            return;
        }

        if (await FlarialLauncher.CheckForUpdatesAsync() && (_settings.AutomaticUpdates || await LauncherUpdateAvailableDialog.ShowAsync()))
        {
            _homePage._button.Content = "Updating...";
            await FlarialLauncher.DownloadAsync(OnFlarialLauncherDownloadAsync);
            return;
        }

        var registry = await VersionRegistry.CreateAsync();
        FrameworkElement homePage = _homePage;
        (~this).Tag = homePage.Tag = registry;

        var task = Task.Run(() =>
        {
            foreach (var item in registry) (~this).Dispatcher.Invoke(() =>
            {
                var listBox = _versionsPage._listBox;
                listBox.Items.Add(item);
            });
        });

        OnPackageStatusChanged();

        _homePage._button.Content = "Play";
        _homePage._button.IsEnabled = true;

        await task;
        _versionsPage._button.IsEnabled = true;
    }
}
using System;
using System.Threading.Tasks;
using Flarial.Launcher.Management;
using Flarial.Launcher.Pages;
using Flarial.Launcher.Runtime.Client;
using Flarial.Launcher.Runtime.Game;
using Flarial.Launcher.Runtime.Versions;
using Flarial.Launcher.Xaml;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Interface;

sealed class MainNavigationView : XamlElement<NavigationView>
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

    readonly PackageCatalog _catalog;
    readonly ApplicationSettings _settings;

    internal MainNavigationView(ApplicationSettings settings) : base(new())
    {
        _settings = settings;
        _catalog = PackageCatalog.OpenForCurrentUser();

        _homePage = new(this, settings);
        _homeItem.Tag = _homePage;

        _versionsPage = new(this);
        _versionsItem.Tag = _versionsPage;

        _settingsPage = new(settings);

        @this.IsPaneOpen = false;
        @this.UseLayoutRounding = true;
        @this.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
        @this.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;

        @this.MenuItems.Add(_homeItem);
        @this.MenuItems.Add(_versionsItem);

        @this.Loaded += OnLoaded;
        @this.ItemInvoked += OnItemInvoked;

        @this.SelectedItem = _homeItem;
        @this.Content = _homePage;
    }

    static void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        var container = args.InvokedItemContainer;
        sender.Content = container.Tag;
    }

    void OnFlarialLauncherDownloadAsync(int value) => @this.Dispatcher.Invoke(() =>
    {
        _homePage._button.Content = $"Updating... {value}%";
    });

    void OnPackageInstalling(PackageCatalog sender, PackageInstallingEventArgs args)
    {
        if (args.IsComplete) OnPackageStatusChanged(args.Package.Id.FamilyName);
    }

    void OnPackageUninstalling(PackageCatalog sender, PackageUninstallingEventArgs args)
    {
        if (args.IsComplete) OnPackageStatusChanged(args.Package.Id.FamilyName);
    }

    void OnPackageUpdating(PackageCatalog sender, PackageUpdatingEventArgs args)
    {
        if (args.IsComplete) OnPackageStatusChanged(args.TargetPackage.Id.FamilyName);
    }

    void OnPackageStatusChanged(string packageFamilyName) => @this.Dispatcher.Invoke(() =>
    {
        if (!packageFamilyName.Equals(Minecraft.PackageFamilyName, StringComparison.OrdinalIgnoreCase))
            return;

        if (!Minecraft.IsInstalled)
        {
            _homePage._leftText.Text = "⚪ 0.0.0";
            return;
        }

        var registry = (VersionRegistry)@this.Tag;
        var text = $"{(registry.IsSupported ? "🟢" : "🔴")} {VersionRegistry.InstalledVersion}";

        _homePage._leftText.Text = text;
    });

    async void OnLoaded(object sender, RoutedEventArgs args)
    {
        var settingsItem = (NavigationViewItem)@this.SettingsItem;
        settingsItem.Tag = _settingsPage;

        if (!await FlarialLauncher.ConnectAsync())
        {
            await MainDialog.ConnectionFailure.ShowAsync(@this);
            System.Windows.Application.Current.Shutdown();
            return;
        }

        if (await FlarialLauncher.CheckAsync() && (_settings.AutomaticUpdates || await MainDialog.LauncherUpdateAvailable.ShowAsync(@this)))
        {
            _homePage._button.Content = "Updating...";
            await FlarialLauncher.DownloadAsync(OnFlarialLauncherDownloadAsync);
            return;
        }

        var registry = await VersionRegistry.CreateAsync();
        FrameworkElement homePage = _homePage;
        @this.Tag = homePage.Tag = registry;

        var task = Task.Run(() =>
        {
            foreach (var item in registry) @this.Dispatcher.Invoke(() =>
            {
                var listBox = _versionsPage._listBox;
                listBox.Items.Add(item);
            });
        });

        OnPackageStatusChanged(Minecraft.PackageFamilyName);

        _catalog.PackageUpdating += OnPackageUpdating;
        _catalog.PackageInstalling += OnPackageInstalling;
        _catalog.PackageUninstalling += OnPackageUninstalling;

        _homePage._button.Content = "Play";
        _homePage._button.IsEnabled = true;

        await task; _versionsPage._button.IsEnabled = true;
    }
}
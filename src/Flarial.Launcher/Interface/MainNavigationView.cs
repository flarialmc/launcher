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

    internal readonly HomePage _homePage;
    internal readonly VersionsPage _versionsPage;
    internal readonly SettingsPage _settingsPage;

    readonly PackageCatalog _catalog;
    readonly ApplicationSettings _settings;

    internal MainNavigationView(ApplicationSettings settings) : base(new())
    {
        _settings = settings;
        _catalog = PackageCatalog.OpenForCurrentUser();

        _homePage = new(this, settings);
        _homeItem.Tag = _homePage._this;

        _versionsPage = new(this);
        _versionsItem.Tag = _versionsPage._this;

        _settingsPage = new(settings);

        _this.IsPaneOpen = false;
        _this.UseLayoutRounding = true;
        _this.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
        _this.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;

        _this.MenuItems.Add(_homeItem);
        _this.MenuItems.Add(_versionsItem);

        _this.Loaded += OnLoaded;
        _this.ItemInvoked += OnItemInvoked;

        _this.SelectedItem = _homeItem;
        _this.Content = _homePage._this;
    }

    static void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        var container = args.InvokedItemContainer;
        sender.Content = container.Tag;
    }

    void OnFlarialLauncherDownloadAsync(int value) => _this.Dispatcher.Invoke(() =>
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

    void OnPackageStatusChanged(string packageFamilyName)
    {
        if (!packageFamilyName.Equals(Minecraft.PackageFamilyName, StringComparison.OrdinalIgnoreCase))
            return;

        _this.Dispatcher.Invoke(() =>
        {
            if (!Minecraft.IsInstalled)
            {
                _homePage._leftText.Text = "❌ 0.0.0";
                return;
            }

            var registry = (VersionRegistry)_this.Tag;
            var text = $"{(registry.IsSupported ? "✔️" : "❌")} {VersionRegistry.InstalledVersion}";

            _homePage._leftText.Text = text;
        });
    }

    async void OnLoaded(object sender, RoutedEventArgs args)
    {
        var settingsItem = (NavigationViewItem)_this.SettingsItem;
        settingsItem.Tag = _settingsPage._this;

        if (!await FlarialLauncher.ConnectAsync())
        {
            await MainDialog.ConnectionFailure.ShowAsync(_this);
            System.Windows.Application.Current.Shutdown();
            return;
        }

        if (await FlarialLauncher.CheckAsync() && (_settings.AutomaticUpdates || await MainDialog.LauncherUpdateAvailable.ShowAsync(_this)))
        {
            _homePage._button.Content = "Updating...";
            await FlarialLauncher.DownloadAsync(OnFlarialLauncherDownloadAsync);
            return;
        }

        var registry = await VersionRegistry.CreateAsync();
        _this.Tag = _homePage._this.Tag = registry;

        var task = Task.Run(() =>
        {
            foreach (var item in registry) _this.Dispatcher.Invoke(() =>
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

        await task;
        _versionsPage._button.IsEnabled = true;
    }
}
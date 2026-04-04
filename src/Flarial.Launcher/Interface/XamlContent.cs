using System;
using System.Threading.Tasks;
using Flarial.Launcher.Controls;
using Flarial.Launcher.Management;
using Flarial.Launcher.Pages;
using Flarial.Launcher.Xaml;
using Flarial.Runtime.Core;
using Flarial.Runtime.Game;
using Flarial.Runtime.Services;
using Flarial.Runtime.Versions;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Interface;

sealed class XamlContent : XamlElement<NavigationView>
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
    readonly AppSettings _settings;

    internal XamlContent(AppSettings settings) : base(new())
    {
        _settings = settings;
        _catalog = PackageCatalog.OpenForCurrentUser();


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
        (~this).Loading += OnLoading;
        (~this).ItemInvoked += OnItemInvoked;

        (~this).Content = _homePage;
        (~this).SelectedItem = _homeItem;
    }

    async void OnLoading(FrameworkElement sender, object args)
    {
        _homePage.Children.Add(new PromotionImagesBox(await PromotionManager.GetDetailsAsync())
        {
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Center
        });
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

    void OnPackageStatusChanged(string packageFamilyName) => (~this).Dispatcher.Invoke(() =>
    {
        if (!packageFamilyName.Equals(Minecraft.PackageFamilyName, StringComparison.OrdinalIgnoreCase))
            return;

        if (!Minecraft.IsInstalled)
        {
            _homePage._leftText.Text = "⚪ 0.0.0";
            return;
        }

        var registry = (VersionRegistry)(~this).Tag;
        var text = $"{(registry.IsSupported ? "🟢" : "🔴")} {VersionRegistry.InstalledVersion}";

        _homePage._leftText.Text = text;
    });

    async void OnLoaded(object sender, RoutedEventArgs args)
    {
        MasterDialog.Current.XamlRoot = (~this).XamlRoot;

        var settingsItem = (NavigationViewItem)(~this).SettingsItem;
        settingsItem.Tag = _settingsPage;

        if (!await FlarialLauncher.VerifyConnectionAsync())
        {
            await DialogRegistry.ConnectionFailure.ShowAsync();
            System.Windows.Application.Current.Shutdown();
            return;
        }

        if (await FlarialLauncher.CheckForUpdatesAsync() && (_settings.AutomaticUpdates || await DialogRegistry.LauncherUpdateAvailable.ShowAsync()))
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

        OnPackageStatusChanged(Minecraft.PackageFamilyName);

        _catalog.PackageUpdating += OnPackageUpdating;
        _catalog.PackageInstalling += OnPackageInstalling;
        _catalog.PackageUninstalling += OnPackageUninstalling;

        _homePage._button.Content = "Play";
        _homePage._button.IsEnabled = true;

        await task; _versionsPage._button.IsEnabled = true;
    }
}
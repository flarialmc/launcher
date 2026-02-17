using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Flarial.Launcher.Interface.Pages;
using Flarial.Launcher.Management;
using Flarial.Launcher.Runtime.Client;
using Flarial.Launcher.Runtime.Game;
using Flarial.Launcher.Runtime.Versions;
using ModernWpf;
using ModernWpf.Controls.Primitives;
using Windows.ApplicationModel;

namespace Flarial.Launcher.Interface;

sealed class AppWindow : Window
{
    void OnPackageInstalling(PackageCatalog sender, PackageInstallingEventArgs args)
    {
        if (!args.IsComplete) return;
        OnPackageStatusChanged(args.Package.Id.FamilyName);
    }

    void OnPackageUninstalling(PackageCatalog sender, PackageUninstallingEventArgs args)
    {
        if (!args.IsComplete) return;
        OnPackageStatusChanged(args.Package.Id.FamilyName);
    }

    void OnPackageUpdating(PackageCatalog sender, PackageUpdatingEventArgs args)
    {
        if (!args.IsComplete) return;
        OnPackageStatusChanged(args.TargetPackage.Id.FamilyName);
    }

    async void OnPackageStatusChanged(string packageFamilyName)
    {
        if (!packageFamilyName.Equals(Minecraft.PackageFamilyName, StringComparison.OrdinalIgnoreCase))
            return;

        Dispatcher.Invoke(() =>
        {
            if (!Minecraft.IsInstalled)
            {
                _homePage._packageVersionTextBlock.Text = "❌ 0.0.0";
                return;
            }

            var registry = (VersionRegistry)Tag;
            var text = $"{(registry.IsSupported ? "✔️" : "❌")} {VersionRegistry.InstalledVersion}";
            _homePage._packageVersionTextBlock.Text = text;
        });
    }

    void InvokeFlarialLauncherDownloadAsync(int value) => Dispatcher.Invoke(() =>
    {
        if (_homePage._progressBar.Value != value)
        {
            _homePage._progressBar.Value = value;
            _homePage._progressBar.IsIndeterminate = false;
        }
    });

    protected override async void OnSourceInitialized(EventArgs args)
    {
        base.OnSourceInitialized(args);

        if (!await FlarialLauncher.ConnectAsync())
        {
            await AppDialog.ConnectionFailure.ShowAsync();
            Application.Current.Shutdown();
            return;
        }

        if (await FlarialLauncher.CheckAsync() && (_configuration.AutomaticUpdates || await AppDialog.LauncherUpdateAvailable.ShowAsync()))
        {
            _homePage._statusTextBlock.Text = "Updating...";
            await FlarialLauncher.DownloadAsync(InvokeFlarialLauncherDownloadAsync);

            _homePage._progressBar.IsIndeterminate = true;
            return;
        }

        var registry = await VersionRegistry.CreateAsync();
        var loadVersionsPageTask = LoadVersionsPageAsync(registry);

        _homePage.Tag = Tag = registry;
        _catalog.PackageUpdating += OnPackageUpdating;
        _catalog.PackageInstalling += OnPackageInstalling;
        _catalog.PackageUninstalling += OnPackageUninstalling;
        OnPackageStatusChanged(Minecraft.PackageFamilyName);

        _homePage.SetVisibility(true);
        await Task.WhenAll(loadVersionsPageTask, _loadLeftSponsorshipTask, _loadCenterSponsorshipTask, _loadRightSponsorshipTask);
    }

    async Task LoadVersionsPageAsync(VersionRegistry registry)
    {
        await Task.Run(() =>
        {
            foreach (var item in registry)
                Dispatcher.Invoke(() => _versionsPage._listBox.Items.Add(item));
        });
        _rootPage._versionsPageItem.IsEnabled = true;
    }


    async Task LoadSponsorshipImageAsync(Task<Tuple<Stream, string>?> task, Image image) => await Dispatcher.InvokeAsync(async () =>
    {
        if (await task is not { } sponsorship)
            return;

        using var stream = sponsorship.Item1;
        var source = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        source.Freeze();

        image.Source = source;
        image.Tag = sponsorship.Item2;
        image.Visibility = Visibility.Visible;

    }, DispatcherPriority.Background);

    readonly Task _loadLeftSponsorshipTask;
    readonly Task _loadCenterSponsorshipTask;
    readonly Task _loadRightSponsorshipTask;

    readonly Task<Tuple<Stream, string>?> _leftSponsorshipTask = Sponsorship.GetAsync<Sponsorship.LiteByteHosting>();
    readonly Task<Tuple<Stream, string>?> _centerSponsorshipTask = Sponsorship.GetAsync<Sponsorship.InfinityNetwork>();
    readonly Task<Tuple<Stream, string>?> _rightSponsorshipTask = Sponsorship.GetAsync<Sponsorship.CollapseNetwork>();

    readonly HomePage _homePage;
    readonly RootPage _rootPage;
    readonly VersionsPage _versionsPage;
    readonly Configuration _configuration;
    readonly PackageCatalog _catalog = PackageCatalog.OpenForCurrentUser();


    internal AppWindow(Configuration configuration)
    {
        _configuration = configuration;

        WindowHelper.SetUseModernWindowStyle(this, true);
        ThemeManager.SetRequestedTheme(this, ElementTheme.Dark);

        Width = 960;
        Height = 540;
        Icon = Manifest.Icon;
        Title = "Flarial Launcher";

        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        SizeToContent = SizeToContent.Manual;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        UseLayoutRounding = true;
        SnapsToDevicePixels = true;
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);

        _rootPage = new(configuration);
        _versionsPage = new(_rootPage);
        _homePage = new(_rootPage, configuration);

        _rootPage._homePageItem.Tag = _homePage;
        _rootPage._versionsPageItem.Tag = _versionsPage;

        _rootPage.Content = _homePage;
        Content = _rootPage;

        _loadLeftSponsorshipTask = LoadSponsorshipImageAsync(_leftSponsorshipTask, _homePage._leftSponsorshipImage);
        _loadCenterSponsorshipTask = LoadSponsorshipImageAsync(_centerSponsorshipTask, _homePage._centerSponsorshipImage);
        _loadRightSponsorshipTask = LoadSponsorshipImageAsync(_rightSponsorshipTask, _homePage._rightSponsorshipImage);
    }
}
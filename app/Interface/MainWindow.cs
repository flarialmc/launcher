using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Flarial.Launcher.Interface.Pages;
using Flarial.Launcher.Management;
using Flarial.Launcher.Services.Game;
using Flarial.Launcher.Services.Versions;
using ModernWpf;
using ModernWpf.Controls.Primitives;
using Windows.ApplicationModel;
using static Flarial.Launcher.Interface.MessageDialog;
using static System.StringComparison;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static System.Windows.Media.Imaging.BitmapCreateOptions;
using static System.Windows.Media.Imaging.BitmapCacheOption;
using System.IO;
using Flarial.Launcher.Services.Client;
using System.Windows.Documents;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;

namespace Flarial.Launcher.Interface;

sealed class MainWindow : Window
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
        if (!packageFamilyName.Equals(Minecraft.PackageFamilyName, OrdinalIgnoreCase))
            return;

        await Dispatcher.InvokeAsync(() =>
        {
            using (Dispatcher.DisableProcessing())
            {
                if (!Minecraft.Installed)
                {
                    _homePage._packageVersionTextBlock.Text = "❌ 0.0.0";
                    return;
                }

                VersionRegistry registry; unsafe
                {
                    var tag = Tag;
                    registry = *(VersionRegistry*)&tag;
                }

                var text = $"{(registry.Supported ? "✔️" : "❌")} {Minecraft.Version}";
                _homePage._packageVersionTextBlock.Text = text;
            }
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

        if (!await FlarialClient.CanConnectAsync() && !await _connectionFailure.ShowAsync())
        {
            Application.Current.Shutdown();
            return;
        }

        if (await FlarialLauncher.CheckAsync() && await _launcherUpdateAvailable.ShowAsync())
        {
            _homePage._statusTextBlock.Text = "Updating...";
            await FlarialLauncher.DownloadAsync(InvokeFlarialLauncherDownloadAsync);
            _homePage._progressBar.IsIndeterminate = true;
            return;
        }

        var registry = await VersionRegistry.CreateAsync();
        object tag; unsafe { tag = *(object*)&registry; }
        Tag = registry; _homePage.Tag = registry;

        ObservableCollection<ListBoxItem> items = [];
        _versionsPage._listBox.ItemsSource = items;

        foreach (var entry in registry) if (entry.Value is { })
        {
            await Dispatcher.Yield(); items.Add(new()
            {
                Tag = entry.Value,
                Content = entry.Key,
                HorizontalContentAlignment = HorizontalAlignment.Center
            });
        }
        _rootPage._versionsPageItem.IsEnabled = true;

        _catalog.PackageUpdating += OnPackageUpdating;
        _catalog.PackageInstalling += OnPackageInstalling;
        _catalog.PackageUninstalling += OnPackageUninstalling;
        OnPackageStatusChanged(Minecraft.PackageFamilyName);

        _homePage.SetVisibility(true);

        /*
            - Dispatch loading sponsorships to dedicated threads.
            - These should improve frontend performance + sponsorship loading.
            - Use `Task.WhenAll` to catch any logic or code issues in production.
        */

        await Task.WhenAll(_loadLeftSponsorshipTask, _loadCenterSponsorshipTask, _loadRightSponsorshipTask);
    }

    void LoadSponsorshipImage(Tuple<Stream, string>? sponsorship, Image image) => Dispatcher.Invoke(() =>
    {
        if (sponsorship is not { } item)
            return;

        using (Dispatcher.DisableProcessing())
        {
            using var stream = item.Item1;

            image.Source = BitmapFrame.Create(stream, PreservePixelFormat, OnLoad);
            image.Source.Freeze();

            var tag = item.Item2;
            unsafe { image.Tag = *(object*)&tag; }

            image.Visibility = Visibility.Visible;
        }
    });

    async Task LoadLeftSponsorshipAsync() => LoadSponsorshipImage(await _leftSponsorshipTask, _homePage._leftSponsorshipImage);
    async Task LoadCenterSponsorshipAsync() => LoadSponsorshipImage(await _centerSponsorshipTask, _homePage._centerSponsorshipImage);
    async Task LoadRightSponsorshipAsync() => LoadSponsorshipImage(await _rightSponsorshipTask, _homePage._rightSponsorshipImage);

    readonly Task _loadLeftSponsorshipTask;
    readonly Task _loadCenterSponsorshipTask;
    readonly Task _loadRightSponsorshipTask;

    readonly Task<Tuple<Stream, string>?> _leftSponsorshipTask = Sponsorship.GetAsync<Sponsorship.LiteByteHosting>();
    readonly Task<Tuple<Stream, string>?> _centerSponsorshipTask = Sponsorship.GetAsync<Sponsorship.InfinityNetwork>();
    readonly Task<Tuple<Stream, string>?> _rightSponsorshipTask = Sponsorship.GetAsync<Sponsorship.CollapseNetwork>();

    readonly HomePage _homePage;
    readonly RootPage _rootPage;
    readonly VersionsPage _versionsPage;
    readonly PackageCatalog _catalog = PackageCatalog.OpenForCurrentUser();

    internal MainWindow(Configuration configuration)
    {
        WindowHelper.SetUseModernWindowStyle(this, true);
        ThemeManager.SetRequestedTheme(this, ElementTheme.Dark);

        Width = 960; Height = 540;
        Title = $"Flarial Launcher";
        Icon = Manifest.Icon;

        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        UseLayoutRounding = true;
        SnapsToDevicePixels = true;
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);

        WindowInteropHelper helper = new(this);

        _rootPage = new(configuration, helper);

        _versionsPage = new(_rootPage);
        _homePage = new(_rootPage, configuration, helper);

        _rootPage._homePageItem.Tag = _homePage;
        _rootPage._versionsPageItem.Tag = _versionsPage;

        _rootPage.Content = _homePage;
        Content = _rootPage;

        _loadLeftSponsorshipTask = Task.Run(LoadLeftSponsorshipAsync);
        _loadCenterSponsorshipTask = Task.Run(LoadCenterSponsorshipAsync);
        _loadRightSponsorshipTask = Task.Run(LoadRightSponsorshipAsync);
    }
}
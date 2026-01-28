using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Flarial.Launcher.Interface.Pages;
using Flarial.Launcher.Management;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Management;
using Flarial.Launcher.Services.Management.Versions;
using Flarial.Launcher.Services.Networking;
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

    void OnPackageStatusChanged(string packageFamilyName)
    {
        if (packageFamilyName.Equals(Minecraft.PackageFamilyName, OrdinalIgnoreCase))
            Dispatcher.Invoke(OnPackageStatusChanged);
    }

    void OnPackageStatusChanged()
    {
        if (!Minecraft.IsInstalled)
        {
            _homePage._packageVersionTextBlock.Text = "❌ 0.0.0";
            return;
        }

        var entries = (VersionRegistry)Tag;
        var text = $"{(entries.IsSupported ? "✔️" : "❌")} {Minecraft.PackageVersion}";
        _homePage._packageVersionTextBlock.Text = text;
    }

    void OnLauncherUpdateDownloadAsync(int value)
    {
        if (!CheckAccess())
        {
            Dispatcher.Invoke(OnLauncherUpdateDownloadAsync, value);
            return;
        }

        if (_homePage._progressBar.Value != value)
        {
            _homePage._progressBar.Value = value;
            _homePage._progressBar.IsIndeterminate = false;
        }
    }

    protected override async void OnSourceInitialized(EventArgs args)
    {
        base.OnSourceInitialized(args);

        if (!await HttpStack.IsAvailableAsync() && !await _connectionFailure.ShowAsync())
            Application.Current.Shutdown();

        if (await LauncherUpdate.CheckAsync() && await _launcherUpdateAvailable.ShowAsync())
        {
            _homePage._statusTextBlock.Text = "Updating...";
            await LauncherUpdate.DownloadAsync(OnLauncherUpdateDownloadAsync);
            _homePage._progressBar.IsIndeterminate = true;
            return;
        }

        var registry = await VersionRegistry.CreateAsync();
        Tag = registry; _homePage.Tag = registry;

        foreach (var item in registry)
        {
            await Dispatcher.Yield();

            if (item.Value is null)
                continue;

            _versionsPage._listBox.Items.Add(new ListBoxItem
            {
                Tag = item.Value,
                Content = item.Key,
                HorizontalContentAlignment = HorizontalAlignment.Center
            });
        }

        _catalog.PackageUpdating += OnPackageUpdating;
        _catalog.PackageInstalling += OnPackageInstalling;
        _catalog.PackageUninstalling += OnPackageUninstalling;

        OnPackageStatusChanged(Minecraft.PackageFamilyName);

        _homePage._progressBar.Value = 0;
        _homePage._progressBar.IsIndeterminate = false;
        _homePage._progressBar.Visibility = Visibility.Collapsed;

        _homePage._statusTextBlock.Text = "Preparing...";
        _homePage._statusTextBlock.Visibility = Visibility.Hidden;

        _homePage._playButton.Visibility = Visibility.Visible;

        _rootPage.IsEnabled = true;

        var leftSponsorshipTask = Task.Run(LoadLeftSponsorshipAsync);
        var centerSponsorshipTask = Task.Run(LoadCenterSponsorshipAsync);
        var rightSponsorshipTask = Task.Run(LoadRightSponsorshipAsync);
        await Task.WhenAll(leftSponsorshipTask, rightSponsorshipTask);
    }

    async Task LoadLeftSponsorshipAsync() => LoadSponsorshipImage(await _leftSponsorshipTask, _homePage._leftSponsorshipImage);
    async Task LoadCenterSponsorshipAsync() => LoadSponsorshipImage(await _centerSponsorshipTask, _homePage._centerSponsorshipImage);
    async Task LoadRightSponsorshipAsync() => LoadSponsorshipImage(await _rightSponsorshipTask, _homePage._rightSponsorshipImage);

    void LoadSponsorshipImage(Tuple<Stream, string>? sponsorship, Image image)
    {
        if (!CheckAccess())
        {
            Dispatcher.Invoke(LoadSponsorshipImage, sponsorship, image);
            return;
        }

        if (sponsorship is not { } item)
            return;

        using var stream = item.Item1;
        image.Source = BitmapFrame.Create(stream, PreservePixelFormat, OnLoad);

        image.Tag = item.Item2;
        image.IsEnabled = true;
    }

    readonly HomePage _homePage;
    readonly RootPage _rootPage;
    readonly VersionsPage _versionsPage;

    readonly Task<Tuple<Stream, string>?> _leftSponsorshipTask = Sponsorship.GetAsync<Sponsorship.LiteByteHosting>();
    readonly Task<Tuple<Stream, string>?> _centerSponsorshipTask = Sponsorship.GetAsync<Sponsorship.InfinityNetwork>();
    readonly Task<Tuple<Stream, string>?> _rightSponsorshipTask = Sponsorship.GetAsync<Sponsorship.CollapseNetwork>();

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
        _homePage = new(configuration, helper);
        _versionsPage = new(_rootPage);

        _rootPage._homePageItem.Tag = _homePage;
        _rootPage._versionsPageItem.Tag = _versionsPage;
        _rootPage.Content = _homePage; Content = _rootPage;
    }
}
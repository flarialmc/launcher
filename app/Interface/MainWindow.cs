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
using System.Collections.Generic;

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

        var entries = (VersionEntries)Tag;
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

        if (!await HttpService.IsAvailableAsync() && !await _connectionFailure.ShowAsync())
            Application.Current.Shutdown();

        if (await LauncherUpdate.CheckAsync() && await _launcherUpdateAvailable.ShowAsync())
        {
            _homePage._statusTextBlock.Text = "Updating...";
            await LauncherUpdate.DownloadAsync(OnLauncherUpdateDownloadAsync);
            _homePage._progressBar.IsIndeterminate = true;
            return;
        }

        var entries = await VersionEntries.CreateAsync();
        Tag = entries; _homePage.Tag = entries;

        foreach (var entry in entries)
        {
            await Dispatcher.Yield();

            if (entry.Value is null)
                continue;

            _versionsPage._listBox.Items.Add(new ListBoxItem
            {
                Tag = entry.Value,
                Content = entry.Key,
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

        await Task.Run(OnSourceFinalizedAsync);
    }

    async Task OnSourceFinalizedAsync()
    {
        /*
            - On a timer, update the sponsorship banners.
            - For now just load the first result, if available.
        */

        await Task.WhenAll(_promosTask, _serversTask);

        var promos = await _promosTask;
        var servers = await _serversTask;

        if (promos.Count > 0)
            LoadSponsorshipImage(promos[0], _homePage._promoSponsorshipImage);

        if (servers.Count > 0)
            LoadSponsorshipImage(servers[0], _homePage._serverSponsorshipImage);
    }

    void LoadSponsorshipImage(SponsorshipBlob blob, Image image)
    {
        if (!CheckAccess())
        {
            Dispatcher.Invoke(LoadSponsorshipImage, blob, image);
            return;
        }

        /*
            - Discard the sponsorship blobs.
            - Tag the image with the campaign Uri.
        */

        using (blob)
        {
            image.Tag = blob._uri;
            image.IsEnabled = true;
            image.Source = BitmapFrame.Create(blob._stream, PreservePixelFormat, OnLoad);
        }
    }

    readonly HomePage _homePage;
    readonly RootPage _rootPage;
    readonly VersionsPage _versionsPage;
    readonly Task<List<SponsorshipBlob>> _promosTask, _serversTask;
    readonly PackageCatalog _catalog = PackageCatalog.OpenForCurrentUser();

    internal MainWindow(Configuration configuration, Task<List<SponsorshipBlob>> promosTask, Task<List<SponsorshipBlob>> serversTask)
    {
        _promosTask = promosTask;
        _serversTask = serversTask;

        WindowHelper.SetUseModernWindowStyle(this, true);
        ThemeManager.SetRequestedTheme(this, ElementTheme.Dark);

        Width = 960; Height = 540;
        Title = $"Flarial Launcher";
        Icon = ApplicationManifest.Icon;

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
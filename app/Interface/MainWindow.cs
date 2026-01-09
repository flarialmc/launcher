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
using static Flarial.Launcher.Interface.MessageDialogContent;
using static System.StringComparison;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static System.Windows.Media.Imaging.BitmapCreateOptions;
using static System.Windows.Media.Imaging.BitmapCacheOption;

namespace Flarial.Launcher.Interface;

sealed class MainWindow : Window
{
    readonly HomePage _homePage;
    readonly RootPage _rootPage;
    readonly VersionsPage _versionsPage;
    readonly WindowInteropHelper _helper;
    readonly PackageCatalog _catalog = PackageCatalog.OpenForCurrentUser();

    internal MainWindow(Configuration configuration)
    {
        _helper = new(this);

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

        _rootPage = new(configuration, _helper);
        _homePage = new(configuration, _helper);
        _versionsPage = new(_rootPage);

        _rootPage._homePageItem.Tag = _homePage;
        _rootPage._versionsPageItem.Tag = _versionsPage;
        Content = _rootPage;

        /*
            - Attempt to the load sponsorship banner & fail sliently if required.
        */

        Dispatcher.InvokeAsync(async () =>
        {
            if (await Sponsorship.StreamAsync() is not { } stream) return;
            _homePage._sponsorshipImage.Source = BitmapFrame.Create(stream, PreservePixelFormat, OnLoad);
            _homePage._sponsorshipImage.IsEnabled = true;
        }, DispatcherPriority.Send);

        SourceInitialized += async (_, _) =>
        {
            var progressBar = (ModernWpf.Controls.ProgressBar)_rootPage.Content;

            /*
                - Verify if an internet connection is available.
            */

            if (!await HttpService.IsAvailableAsync() &&
                !await MessageDialog.ShowAsync(_connectionFailure))
                Application.Current.Shutdown();

            /*
                - Check for launcher updates & download them.
            */


            if (await LauncherUpdater.CheckAsync() &&
                await MessageDialog.ShowAsync(_launcherUpdateAvailable))
            {
                await LauncherUpdater.DownloadAsync((_) => Dispatcher.Invoke(() =>
                {
                    if (progressBar.Value == _) return;
                    progressBar.Value = _; progressBar.IsIndeterminate = false;
                }, DispatcherPriority.Send));

                progressBar.IsIndeterminate = true; return;
            }

            var entries = await VersionEntries.CreateAsync();
            _homePage.Tag = entries;

            foreach (var entry in entries)
            {
                await Dispatcher.Yield(); if (entry.Value is null) continue;
                _versionsPage._listBox.Items.Add(new ListBoxItem
                {
                    Tag = entry.Value,
                    Content = entry.Key,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                });
            }

            /*
                - Setup package version updates.
            */

            void OnPackageStatusChanged(string packageFamilyName) => Dispatcher.Invoke(() =>
            {
                if (!packageFamilyName.Equals(Minecraft.PackageFamilyName, OrdinalIgnoreCase)) return;
                if (!Minecraft.IsInstalled) { _homePage._packageVersionTextBlock.Text = "❌ 0.0.0"; return; }
                _homePage._packageVersionTextBlock.Text = $"{(entries.IsSupported ? "✔️" : "❌")} {Minecraft.PackageVersion}";
            }, DispatcherPriority.Send);

            _catalog.PackageInstalling += (sender, args) => { if (args.IsComplete) OnPackageStatusChanged(args.Package.Id.FamilyName); };
            _catalog.PackageUninstalling += (sender, args) => { if (args.IsComplete) OnPackageStatusChanged(args.Package.Id.FamilyName); };
            _catalog.PackageUpdating += (sender, args) => { if (args.IsComplete) OnPackageStatusChanged(args.TargetPackage.Id.FamilyName); };

            /*
                - Finalize initialization.
            */

            OnPackageStatusChanged(Minecraft.PackageFamilyName);
            _rootPage.IsEnabled = true; _rootPage.IsPaneVisible = true;
            _rootPage._homePageItem.IsSelected = true; _rootPage.Content = _homePage;
        };
    }
}
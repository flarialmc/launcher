using System.Windows;
using System.Windows.Media;
using ModernWpf.Controls;
using System.Windows.Controls;
using Flarial.Launcher.Services.Management.Versions;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Client;
using Flarial.Launcher.Services.Modding;
using static Flarial.Launcher.Interface.MessageDialogContent;
using System.Windows.Threading;
using System.Linq;
using Windows.ApplicationModel;
using System;
using Flarial.Launcher.Management;
using static System.StringComparison;
using System.Text;

namespace Flarial.Launcher.Interface.Pages;

sealed class HomePage : Grid
{
    readonly Image _logoImage = new()
    {
        Source = ApplicationManifest.Icon,
        Width = ApplicationManifest.Icon.Width / 3,
        Height = ApplicationManifest.Icon.Height / 3,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new(0, 0, 0, 120)
    };

    readonly ModernWpf.Controls.ProgressBar _progressBar = new()
    {
        Width = ApplicationManifest.Icon.Width * 2,
        Foreground = new SolidColorBrush(Colors.White),
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new(0, 120, 0, 0),
        Visibility = Visibility.Hidden
    };

    readonly TextBlock _statusTextBlock = new()
    {
        Text = "Preparing...",
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new(0, 60, 0, 0),
        Visibility = Visibility.Hidden
    };

    readonly Button _launchButton = new()
    {
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Content = "Launch",
        Width = ApplicationManifest.Icon.Width * 2,
        Margin = new(0, 120, 0, 0)
    };

    readonly TextBlock _packageVersionTextBlock = new()
    {
        Text = "0.0.0",
        VerticalAlignment = VerticalAlignment.Bottom,
        HorizontalAlignment = HorizontalAlignment.Left,
        Margin = new(12, 0, 0, 12)
    };

    readonly TextBlock _launcherVersionTextBlock = new()
    {
        Text = ApplicationManifest.Version,
        VerticalAlignment = VerticalAlignment.Bottom,
        HorizontalAlignment = HorizontalAlignment.Right,
        Margin = new(0, 0, 12, 12)
    };

    sealed class UnsupportedVersionDetected(string packageVersion, string supportedVersion) : MessageDialogContent
    {
        public override string Title => "⚠️ Unsupported Version Detected";
        public override string Primary => "Back";
        public override string Content => $@"Minecraft v{packageVersion} isn't compatible with Flarial Client.

• Please switch to Minecraft v{supportedVersion} for the best experience.
• You may switch versions by going to the [Versions] page in the launcher.

If you need help, join our Discord.";
    }

    readonly VersionEntries _entries;
    readonly Configuration _configuration;
    readonly PackageCatalog _catalog = PackageCatalog.OpenForCurrentUser();

    internal HomePage(Configuration configuration, VersionEntries entries, Task<Image?> sponsorship)
    {
        Children.Add(_logoImage);
        Children.Add(_progressBar);
        Children.Add(_statusTextBlock);
        Children.Add(_launchButton);
        Children.Add(_launcherVersionTextBlock);
        Children.Add(_packageVersionTextBlock);

        _entries = entries;
        _configuration = configuration;

        UpdatePackageVersionText();
        _catalog.PackageUpdating += OnPackageUpdating;
        _catalog.PackageInstalling += OnPackageInstalling;
        _catalog.PackageUninstalling += OnPackageUninstalling;

        _launchButton.Click += OnLaunchButtonClick;

        Dispatcher.Invoke(DispatcherPriority.Send, InitializeAsync, sponsorship);
    }

    async void InitializeAsync(Task<Image?> sponsorship)
    {
        var image = await sponsorship;
        if (image is { }) Children.Add(image);
    }

    void OnPackageInstalling(PackageCatalog sender, PackageInstallingEventArgs args)
    {
        if (args.IsComplete)
            HasPackageStatusChanged(args.Package);
    }

    void OnPackageUpdating(PackageCatalog sender, PackageUpdatingEventArgs args)
    {
        if (args.IsComplete)
            HasPackageStatusChanged(args.TargetPackage);
    }

    void OnPackageUninstalling(PackageCatalog sender, PackageUninstallingEventArgs args)
    {
        if (args.IsComplete)
            HasPackageStatusChanged(args.Package);
    }

    void HasPackageStatusChanged(Package package)
    {
        if (package.Id.FamilyName.Equals(Minecraft.PackageFamilyName, OrdinalIgnoreCase))
            Dispatcher.Invoke(DispatcherPriority.Send, UpdatePackageVersionText);
    }

    void UpdatePackageVersionText()
    {
        try
        {
            var supported = _entries.IsSupported ? "✔️" : "❌";
            var packageVersion = Minecraft.PackageVersion;

            StringBuilder builder = new();

            builder.Append(supported);
            builder.Append(" ");
            builder.Append(packageVersion);

            _packageVersionTextBlock.Text = $"{builder}";
        }
        catch { _packageVersionTextBlock.Text = "0.0.0"; }
    }

    void InvokeOnDownloadProgress(int value) => Dispatcher.Invoke(DispatcherPriority.Send, OnDownloadProgress, value);

    async void OnDownloadProgress(int value)
    {
        if (_progressBar.Value == value) return;
        _statusTextBlock.Text = "Downloading...";

        _progressBar.Value = value;
        _progressBar.IsIndeterminate = false;
    }

    async void OnLaunchButtonClick(object sender, RoutedEventArgs args)
    {
        try
        {
            _launchButton.Visibility = Visibility.Hidden;
            _progressBar.IsIndeterminate = true;
            _progressBar.Visibility = Visibility.Visible;
            _statusTextBlock.Visibility = Visibility.Visible;

            if (!Minecraft.IsInstalled)
            {
                await MessageDialog.ShowAsync(_notInstalled);
                return;
            }

            if (Minecraft.UsingGameDevelopmentKit && !Minecraft.IsPackaged)
            {
                await MessageDialog.ShowAsync(_unsignedInstallationDetected);
                return;
            }

            var path = _configuration.CustomDllPath;
            var beta = _configuration.DllBuild is DllBuild.Beta;
            var custom = _configuration.DllBuild is DllBuild.Custom;
            var initialized = _configuration.WaitForInitialization;
            var client = beta ? FlarialClient.Beta : FlarialClient.Release;

            if (!custom && !beta && !_entries.IsSupported)
            {
                var packageVersion = Minecraft.PackageVersion;
                var supportedVersion = _entries.First().Key;

                UnsupportedVersionDetected unsupportedVersionDetected = new(packageVersion, supportedVersion);
                await MessageDialog.ShowAsync(unsupportedVersionDetected); return;
            }

            if (custom)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    await MessageDialog.ShowAsync(_invalidCustomDll);
                    return;
                }

                ModificationLibrary library = new(path!);

                if (!library.IsValid)
                {
                    await MessageDialog.ShowAsync(_invalidCustomDll);
                    return;
                }

                _statusTextBlock.Text = "Launching...";

                if (await Task.Run(() => Injector.Launch(initialized, library)) is null)
                {
                    await MessageDialog.ShowAsync(_launchFailure);
                    return;
                }

                return;
            }

            _statusTextBlock.Text = "Verifying...";

            if (!await client.DownloadAsync(InvokeOnDownloadProgress))
            {
                await MessageDialog.ShowAsync(_clientUpdateFailure);
                return;
            }

            _statusTextBlock.Text = "Launching...";
            _progressBar.IsIndeterminate = true;

            if (beta && await MessageDialog.ShowAsync(_betaDllEnabled))
                return;

            if (!await Task.Run(() => client.Launch(initialized)))
            {
                await MessageDialog.ShowAsync(_launchFailure);
                return;
            }
        }
        finally
        {
            _statusTextBlock.Text = "Preparing...";
            _progressBar.IsIndeterminate = false;
            _progressBar.Visibility = Visibility.Hidden;
            _statusTextBlock.Visibility = Visibility.Hidden;
            _launchButton.Visibility = Visibility.Visible;
        }
    }
}
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
        Text = "❌ 0.0.0",
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

    sealed class UnsupportedVersion(string packageVersion, string supportedVersion) : MessageDialogContent
    {
        public override string Title => "⚠️ Unsupported Version";
        public override string Primary => "Back";
        public override string Content => $@"Minecraft {packageVersion} isn't compatible with Flarial Client.

• Please switch to Minecraft {supportedVersion} for the best experience.
• You may switch versions by going to the [Versions] page in the launcher.

If you need help, join our Discord.";
    }


    readonly PackageCatalog _catalog = PackageCatalog.OpenForCurrentUser();

    internal HomePage(Configuration configuration, VersionEntries entries, Task<Image?> sponsorship)
    {
        Children.Add(_logoImage);
        Children.Add(_progressBar);
        Children.Add(_statusTextBlock);
        Children.Add(_launchButton);
        Children.Add(_launcherVersionTextBlock);
        Children.Add(_packageVersionTextBlock);

        void OnPackageStatusChanged(string packageFamilyName) => Dispatcher.Invoke(() =>
        {
            if (!packageFamilyName.Equals(Minecraft.PackageFamilyName, OrdinalIgnoreCase)) return;
            try { _packageVersionTextBlock.Text = $"{(entries.IsSupported ? "✔️" : "❌")} {Minecraft.PackageVersion}"; }
            catch { _packageVersionTextBlock.Text = "❌ 0.0.0"; }
        }, DispatcherPriority.Send);

        _catalog.PackageUpdating += (sender, args) => { if (args.IsComplete) OnPackageStatusChanged(args.TargetPackage.Id.FamilyName); };
        _catalog.PackageInstalling += (sender, args) => { if (args.IsComplete) OnPackageStatusChanged(args.Package.Id.FamilyName); };
        _catalog.PackageUninstalling += (sender, args) => { if (args.IsComplete) OnPackageStatusChanged(args.Package.Id.FamilyName); };

        OnPackageStatusChanged(Minecraft.PackageFamilyName);

        Dispatcher.Invoke(async () =>
        {
            var image = await sponsorship;
            if (image is { }) Children.Add(image);
        }, DispatcherPriority.Send);

        _launchButton.Click += async (_, _) =>
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
                    await MessageDialog.ShowAsync(_unsignedInstallation);
                    return;
                }

                var path = configuration.CustomDllPath;
                var beta = configuration.DllBuild is DllBuild.Beta;
                var custom = configuration.DllBuild is DllBuild.Custom;
                var initialized = configuration.WaitForInitialization;
                var client = beta ? FlarialClient.Beta : FlarialClient.Release;

                if (!custom && !beta && !entries.IsSupported)
                {
                    var packageVersion = Minecraft.PackageVersion;
                    var supportedVersion = entries.First().Key;

                    await MessageDialog.ShowAsync(new UnsupportedVersion(Minecraft.PackageVersion, entries.First().Key)); return;
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

                if (beta && await MessageDialog.ShowAsync(_betaDllEnabled))
                    return;

                _statusTextBlock.Text = "Verifying...";

                if (!await client.DownloadAsync((_) => Dispatcher.Invoke(() =>
                {
                    if (_progressBar.Value == _) return;
                    _statusTextBlock.Text = "Downloading...";

                    _progressBar.Value = _;
                    _progressBar.IsIndeterminate = false;
                }, DispatcherPriority.Send)))
                {
                    await MessageDialog.ShowAsync(_clientUpdateFailure);
                    return;
                }

                _statusTextBlock.Text = "Launching...";
                _progressBar.IsIndeterminate = true;

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
        };
    }
}
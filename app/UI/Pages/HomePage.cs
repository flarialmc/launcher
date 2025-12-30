using System.Windows;
using System.Windows.Media;
using Flarial.Launcher.App;
using ModernWpf.Controls;
using System.Windows.Controls;
using Flarial.Launcher.Services.Management.Versions;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Client;
using Flarial.Launcher.Services.Modding;
using System.Windows.Threading;
using System.Linq;
using Windows.ApplicationModel;
using System;

namespace Flarial.Launcher.UI.Pages;

sealed class HomePage : Grid
{
    readonly Image _logo = new()
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

    readonly TextBlock _status = new()
    {
        Text = "Preparing...",
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new(0, 60, 0, 0),
        Visibility = Visibility.Hidden
    };

    readonly Button _button = new()
    {
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Content = "Launch",
        Width = ApplicationManifest.Icon.Width * 2,
        Margin = new(0, 120, 0, 0)
    };

    readonly TextBlock _version = new()
    {
        Text = "0.0.0",
        VerticalAlignment = VerticalAlignment.Bottom,
        HorizontalAlignment = HorizontalAlignment.Left,
        Margin = new(12, 0, 0, 12)
    };

    sealed class UnsupportedVersionDetected(string installed, string supported) : MessageDialogContent
    {
        public override string Title => "⚠️ Unsupported Version Detected";
        public override string Primary => "Back";
        public override string Content => $@"Minecraft v{installed} isn't compatible with Flarial Client.

• Please switch to Minecraft v{supported} for the best experience.
• You may switch versions by going to the [Versions] page in the launcher.

If you need help, join our Discord.";
    }

    readonly PackageCatalog _packageCatalog = PackageCatalog.OpenForCurrentUser();
    void PackageStatusChanged(VersionCatalog catalog) => Dispatcher.Invoke(() =>
    {
        try { _version.Text = $"{(catalog.IsSupported ? "✔️" : "❌")} {Minecraft.PackageVersion}"; }
        catch { _version.Text = "0.0.0"; }
    }, DispatcherPriority.Send);

    internal HomePage(Configuration configuration, VersionCatalog catalog, Task<Image?> sponsorship)
    {
        Children.Add(_logo);
        Children.Add(_progressBar);
        Children.Add(_status);
        Children.Add(_button);
        Children.Add(_version);
        Children.Add(new TextBlock
        {
            Text = ApplicationManifest.Version,
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new(0, 0, 12, 12)
        });

        PackageStatusChanged(catalog);

        _packageCatalog.PackageUpdating += (sender, args) =>
        {
            if (args.IsComplete)
                PackageStatusChanged(catalog);
        };

        _packageCatalog.PackageInstalling += (sender, args) =>
        {
            if (args.IsComplete)
                PackageStatusChanged(catalog);
        };

        _packageCatalog.PackageUninstalling += (sender, args) =>
        {
            if (args.IsComplete)
                PackageStatusChanged(catalog);
        };

        Dispatcher.Invoke(async () =>
        {
            var image = await sponsorship;
            if (image is { }) Children.Add(image);
        }, DispatcherPriority.Send);

        _button.Click += async (_, _) =>
        {
            try
            {
                _button.Visibility = Visibility.Hidden;

                _progressBar.IsIndeterminate = true;
                _progressBar.Visibility = Visibility.Visible;

                _status.Visibility = Visibility.Visible;

                if (!Minecraft.IsInstalled)
                {
                    await MessageDialog.ShowAsync(MessageDialogContent._notInstalled);
                    return;
                }

                if (Minecraft.UsingGameDevelopmentKit && !Minecraft.IsPackaged)
                {
                    await MessageDialog.ShowAsync(MessageDialogContent._unsignedInstallationDetected);
                    return;
                }

                var beta = configuration.DllBuild is DllBuild.Beta;
                var custom = configuration.DllBuild is DllBuild.Custom;
                var client = beta ? FlarialClient.Beta : FlarialClient.Release;

                if (!custom && !beta && !catalog.IsSupported)
                {
                    await MessageDialog.ShowAsync(new UnsupportedVersionDetected(Minecraft.PackageVersion, catalog.First().Key));
                    return;
                }

                if (custom)
                {
                    if (string.IsNullOrWhiteSpace(configuration.CustomDllPath))
                    {
                        await MessageDialog.ShowAsync(MessageDialogContent._invalidCustomDll);
                        return;
                    }

                    ModificationLibrary library = new(configuration.CustomDllPath!);

                    if (!library.IsValid)
                    {
                        await MessageDialog.ShowAsync(MessageDialogContent._invalidCustomDll);
                        return;
                    }

                    _status.Text = "Launching...";

                    if (await Task.Run(() => Injector.Launch(configuration.WaitForInitialization, library)) is null)
                    {
                        await MessageDialog.ShowAsync(MessageDialogContent._launchFailure);
                        return;
                    }

                    return;
                }

                _status.Text = "Verifying...";

                if (!await client.DownloadAsync(_ => Dispatcher.Invoke(() =>
                {
                    if (_progressBar.Value == _) return;
                    _status.Text = "Downloading...";

                    _progressBar.Value = _;
                    _progressBar.IsIndeterminate = false;
                }, DispatcherPriority.Send)))
                {
                    await MessageDialog.ShowAsync(MessageDialogContent._clientUpdateFailure);
                    return;
                }

                _status.Text = "Launching...";
                _progressBar.IsIndeterminate = true;

                if (beta && await MessageDialog.ShowAsync(MessageDialogContent._betaDllEnabled))
                    return;

                if (!await Task.Run(() => client.Launch(configuration.WaitForInitialization)))
                {
                    await MessageDialog.ShowAsync(MessageDialogContent._launchFailure);
                    return;
                }
            }
            finally
            {
                _progressBar.IsIndeterminate = false;
                _progressBar.Visibility = Visibility.Hidden;

                _status.Text = "Preparing...";
                _status.Visibility = Visibility.Hidden;

                _button.Visibility = Visibility.Visible;
            }
        };
    }
}
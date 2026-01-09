using System.Windows;
using System.Windows.Media;
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
using ModernWpf.Controls;
using System.Windows.Input;
using System.Windows.Interop;

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
        Width = ApplicationManifest.Icon.Width,
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

    readonly Button _playButton = new()
    {
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Content = "Play",
        Width = ApplicationManifest.Icon.Width,
        Margin = new(0, 120, 0, 0)
    };

    internal readonly TextBlock _packageVersionTextBlock = new()
    {
        Text = "❌ 0.0.0",
        VerticalAlignment = VerticalAlignment.Bottom,
        HorizontalAlignment = HorizontalAlignment.Left,
        Margin = new(12, 0, 0, 12)
    };

    readonly TextBlock _launcherVersionTextBlock = new()
    {
        Text = ApplicationManifest.Version,
        Margin = new(0, 0, 12, 12),
        VerticalAlignment = VerticalAlignment.Bottom,
        HorizontalAlignment = HorizontalAlignment.Right
    };

    readonly HyperlinkButton _discordLinkButton = new()
    {
        Content = "Discord",
        Foreground = new SolidColorBrush(Colors.White),
        Padding = new(),
        NavigateUri = new("https://flarial.xyz/discord"),
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalAlignment = HorizontalAlignment.Right,
        Margin = new(0, 12, 12, 0)
    };

    internal readonly Image _sponsorshipImage = new()
    {
        VerticalAlignment = VerticalAlignment.Bottom,
        HorizontalAlignment = HorizontalAlignment.Center,
        Height = 50,
        Width = 320,
        Margin = new(0, 0, 0, 12),
        Cursor = Cursors.Hand,
        IsEnabled = false
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

    //    readonly PackageCatalog _catalog = PackageCatalog.OpenForCurrentUser();

    internal HomePage(Configuration configuration, WindowInteropHelper helper)
    {
        Children.Add(_logoImage);
        Children.Add(_progressBar);
        Children.Add(_statusTextBlock);
        Children.Add(_discordLinkButton);
        Children.Add(_playButton);
        Children.Add(_launcherVersionTextBlock);
        Children.Add(_packageVersionTextBlock);
        Children.Add(_sponsorshipImage);

        _sponsorshipImage.MouseLeftButtonDown += (_, _) => PInvoke.ShellExecute(helper.EnsureHandle(), null!, Sponsorship.CampaignUri, null!, null!, PInvoke.SW_NORMAL);

        _playButton.Click += async (_, _) =>
        {
            try
            {
                _playButton.Visibility = Visibility.Hidden;
                _progressBar.IsIndeterminate = true;
                _progressBar.Visibility = Visibility.Visible;
                _statusTextBlock.Visibility = Visibility.Visible;

                var entries = (VersionEntries)Tag;

                if (!Minecraft.IsInstalled)
                {
                    await MessageDialog.ShowAsync(_notInstalled);
                    return;
                }

                if (Minecraft.UsingGameDevelopmentKit && !Minecraft.IsPackaged)
                {
                    if (!Minecraft.AllowUnsignedInstalls) { await MessageDialog.ShowAsync(_unsignedInstall); return; }
                    else if (await MessageDialog.ShowAsync(_allowUnsignedInstalls)) return;
                }

                var path = configuration.CustomDllPath;
                var beta = configuration.DllBuild is DllBuild.Beta;
                var custom = configuration.DllBuild is DllBuild.Custom;
                var initialized = configuration.WaitForInitialization;
                var client = beta ? FlarialClient.Beta : FlarialClient.Release;

                if (!custom && !beta && !entries.IsSupported)
                {
                    await MessageDialog.ShowAsync(new UnsupportedVersion(Minecraft.PackageVersion, entries.First().Key));
                    return;
                }

                if (custom)
                {
                    if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
                    {
                        await MessageDialog.ShowAsync(_invalidCustomDll);
                        return;
                    }

                    Library library = new(path); if (!library.IsLoadable)
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
                _playButton.Visibility = Visibility.Visible;
            }
        };
    }
}
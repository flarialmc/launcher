using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using Flarial.Launcher.Services.Management.Versions;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Client;
using Flarial.Launcher.Services.Modding;
using static Flarial.Launcher.Interface.MessageDialog;
using System.Windows.Threading;
using System.Linq;
using System;
using Flarial.Launcher.Management;
using ModernWpf.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace Flarial.Launcher.Interface.Pages;

sealed class HomePage : Grid
{
    readonly WindowInteropHelper _helper;
    readonly Configuration _configuration;

    readonly Image _logoImage = new()
    {
        Source = ApplicationManifest.Icon,
        Width = ApplicationManifest.Icon.Width / 2.5,
        Height = ApplicationManifest.Icon.Height / 2.5,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new(0, 0, 0, 120)
    };

    internal readonly ModernWpf.Controls.ProgressBar _progressBar = new()
    {
        Width = ApplicationManifest.Icon.Width,
        Foreground = new SolidColorBrush(Colors.White),
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new(0, 90, 0, 0),
        IsIndeterminate = true
    };

    internal readonly TextBlock _statusTextBlock = new()
    {
        Text = "Preparing...",
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new(0, 30, 0, 0)
    };

    internal readonly Button _playButton = new()
    {
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Content = "Play",
        Width = ApplicationManifest.Icon.Width,
        Margin = new(0, 90, 0, 0),
        Visibility = Visibility.Hidden
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

    sealed class UnsupportedVersion(string packageVersion, string supportedVersion) : MessageDialog
    {
        protected override string Title => "⚠️ Unsupported Version";
        protected override string Primary => "Back";
        protected override string Content => $@"Minecraft {packageVersion} isn't compatible with Flarial Client.

• Please switch to Minecraft {supportedVersion} for the best experience.
• You may switch versions by going to the [Versions] page in the launcher.

If you need help, join our Discord.";
    }

    void OnSponsorshipImageClick(object sender, EventArgs args) => PInvoke.ShellExecute(_helper.EnsureHandle(), null!, Sponsorship.CampaignUri, null!, null!, PInvoke.SW_NORMAL);

    async void OnFlarialClientDownloadAsync(int value)
    {
        if (!CheckAccess())
        {
            Dispatcher.Invoke(DispatcherPriority.Send, OnFlarialClientDownloadAsync, value);
            return;
        }

        if (_progressBar.Value != value)
        {
            _progressBar.Value = value;
            _progressBar.IsIndeterminate = false;
        }

        _statusTextBlock.Text = "Downloading...";
    }

    async void OnPlayButtonClick(object sender, EventArgs args)
    {
        try
        {
            _playButton.Visibility = Visibility.Hidden;

            _progressBar.IsIndeterminate = true;
            _progressBar.Visibility = Visibility.Visible;

            _statusTextBlock.Text = "Preparing...";
            _statusTextBlock.Visibility = Visibility.Visible;

            var entries = (VersionEntries)Tag;

            if (!Minecraft.IsInstalled)
            {
                await _notInstalled.ShowAsync();
                return;
            }

            if (Minecraft.UsingGameDevelopmentKit && !Minecraft.IsPackaged)
            {
                if (!Minecraft.AllowUnsignedInstalls)
                {
                    await _unsignedInstall.ShowAsync();
                    return;
                }
                else if (await _allowUnsignedInstalls.ShowAsync())
                    return;
            }

            var path = _configuration.CustomDllPath;
            var beta = _configuration.DllBuild is DllBuild.Beta;
            var custom = _configuration.DllBuild is DllBuild.Custom;
            var initialized = _configuration.WaitForInitialization;
            var client = beta ? FlarialClient.Beta : FlarialClient.Release;

            if (!custom && !beta && !entries.IsSupported)
            {
                await new UnsupportedVersion(Minecraft.PackageVersion, entries.First().Key).ShowAsync();
                return;
            }

            if (custom)
            {
                if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
                {
                    await _invalidCustomDll.ShowAsync();
                    return;
                }

                Library library = new(path);

                if (!library.IsLoadable)
                {
                    await _invalidCustomDll.ShowAsync();
                    return;
                }

                _statusTextBlock.Text = "Launching...";

                if (await Task.Run(() => Injector.Launch(initialized, library)) is null)
                {
                    await _launchFailure.ShowAsync();
                    return;
                }

                return;
            }

            if (beta && await _betaDllEnabled.ShowAsync())
                return;

            _statusTextBlock.Text = "Verifying...";

            if (!await client.DownloadAsync(OnFlarialClientDownloadAsync))
            {
                await _clientUpdateFailure.ShowAsync();
                return;
            }

            _statusTextBlock.Text = "Launching...";
            _progressBar.IsIndeterminate = true;

            if (!await Task.Run(() => client.Launch(initialized)))
            {
                await _launchFailure.ShowAsync();
                return;
            }
        }
        finally
        {
            _progressBar.IsIndeterminate = false;
            _progressBar.Visibility = Visibility.Hidden;

            _statusTextBlock.Text = "Preparing...";
            _statusTextBlock.Visibility = Visibility.Hidden;

            _playButton.Visibility = Visibility.Visible;
        }
    }

    internal HomePage(Configuration configuration, WindowInteropHelper helper)
    {
        _helper = helper;
        _configuration = configuration;

        Children.Add(_logoImage);
        Children.Add(_progressBar);
        Children.Add(_statusTextBlock);
        Children.Add(_discordLinkButton);
        Children.Add(_playButton);
        Children.Add(_launcherVersionTextBlock);
        Children.Add(_packageVersionTextBlock);
        Children.Add(_sponsorshipImage);

        _playButton.Click += OnPlayButtonClick;
        _sponsorshipImage.MouseLeftButtonDown += OnSponsorshipImageClick;
    }
}
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Game;
using Flarial.Launcher.Services.Client;
using Flarial.Launcher.Services.Modding;
using static Flarial.Launcher.Interface.MessageDialog;
using System.Windows.Threading;
using System.Linq;
using System;
using Flarial.Launcher.Management;
using System.Windows.Input;
using System.Windows.Interop;
using Flarial.Launcher.Services.Versions;
using ModernWpf.Controls;

namespace Flarial.Launcher.Interface.Pages;

sealed class HomePage : Grid
{
    readonly RootPage _rootPage;
    readonly WindowInteropHelper _helper;
    readonly Configuration _configuration;

    readonly Image _logoImage = new()
    {
        Source = Manifest.Icon,
        Width = Manifest.Icon.Width / 2.5,
        Height = Manifest.Icon.Height / 2.5,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new(0, 0, 0, 120)
    };

    internal readonly ModernWpf.Controls.ProgressBar _progressBar = new()
    {
        Width = Manifest.Icon.Width,
        Foreground = new SolidColorBrush(Colors.White),
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new(0, 90, 0, 0),
        IsIndeterminate = true
    };

    internal readonly TextBlock _statusTextBlock = new()
    {
        Text = "Connecting...",
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new(0, 30, 0, 0)
    };

    internal readonly Button _playButton = new()
    {
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Content = "Play",
        Width = Manifest.Icon.Width,
        Margin = new(0, 90, 0, 0),
        Visibility = Visibility.Collapsed
    };

    internal readonly TextBlock _packageVersionTextBlock = new()
    {
        Text = "❌ 0.0.0",
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalAlignment = HorizontalAlignment.Left,
        Margin = new(12, 12, 0, 0)
    };

    readonly TextBlock _launcherVersionTextBlock = new()
    {
        Text = Manifest.Version,
        Margin = new(0, 12, 12, 0),
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalAlignment = HorizontalAlignment.Right
    };

    internal readonly Image _leftSponsorshipImage = new()
    {
        Stretch = Stretch.UniformToFill,
        VerticalAlignment = VerticalAlignment.Bottom,
        HorizontalAlignment = HorizontalAlignment.Left,
        Height = 50 * 0.95,
        Width = 320 * 0.95,
        Margin = new(12, 0, 0, 12),
        Cursor = Cursors.Hand,
        Visibility = Visibility.Collapsed
    };

    internal readonly Image _centerSponsorshipImage = new()
    {
        Stretch = Stretch.UniformToFill,
        VerticalAlignment = VerticalAlignment.Bottom,
        HorizontalAlignment = HorizontalAlignment.Center,
        Height = 50 * 0.95,
        Width = 320 * 0.95,
        Margin = new(0, 0, 0, 12),
        Cursor = Cursors.Hand,
        Visibility = Visibility.Collapsed
    };

    internal readonly Image _rightSponsorshipImage = new()
    {
        Stretch = Stretch.UniformToFill,
        VerticalAlignment = VerticalAlignment.Bottom,
        HorizontalAlignment = HorizontalAlignment.Right,
        Height = 50 * 0.95,
        Width = 320 * 0.95,
        Margin = new(0, 0, 12, 12),
        Cursor = Cursors.Hand,
        Visibility = Visibility.Collapsed
    };

    sealed class UnsupportedVersion(string version, string preferred) : MessageDialog
    {
        protected override string Close => "Back";
        protected override string Primary => "Versions";
        protected override string? Secondary => "Settings";
        protected override string Title => "⚠️ Unsupported Version";
        protected override string Content => $@"Minecraft {version} isn't supported by Flarial Client.

• Switch to {preferred} on the [Versions] page.
• Enable the client's Beta on the [Settings] page.

If you need help, join our Discord.";
    }

    /*
        - In the future, the sponsorship banners should rotate.
        - Use WPF's `FrameworkElement.Tag` property to attach clickable metadata.
    */

    void ShellExecute(string lpFile) => PInvoke.ShellExecute(_helper.EnsureHandle(), null!, lpFile, null!, null!, PInvoke.SW_NORMAL);

    unsafe void OnSponsorshipImageClick(object sender, EventArgs args)
    {
        var tag = (*(FrameworkElement*)&sender).Tag;
        ShellExecute(*(string*)&tag);
    }

    void OnFlarialClientDownloadAsync(int value) => Dispatcher.Invoke(() =>
    {
        if (_progressBar.Value != value)
        {
            _progressBar.Value = value;
            _progressBar.IsIndeterminate = false;
        }
        _statusTextBlock.Text = "Downloading...";
    });

    internal void SetVisibility(bool visible)
    {
        using (Dispatcher.DisableProcessing())
        {
            _playButton.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

            _progressBar.Value = 0; _progressBar.IsIndeterminate = !visible;
            _progressBar.Visibility = visible ? Visibility.Collapsed : Visibility.Visible;

            _statusTextBlock.Text = "Preparing...";
            _statusTextBlock.Visibility = visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    async void OnPlayButtonClick(object sender, EventArgs args)
    {
        try
        {
            if (!Minecraft.Installed)
            {
                await _notInstalled.ShowAsync();
                return;
            }

            if (Minecraft.UsingGameDevelopmentKit && !Minecraft.Packaged)
            {
                if (!Minecraft.AllowUnsignedInstalls)
                {
                    await _unsignedInstall.ShowAsync();
                    return;
                }
                else if (await _allowUnsignedInstalls.ShowAsync())
                    return;
            }

            VersionRegistry registry; unsafe
            {
                var tag = Tag;
                registry = *(VersionRegistry*)&tag;
            }

            var path = _configuration.CustomDllPath;
            var beta = _configuration.DllBuild is DllBuild.Beta;
            var initialized = _configuration.WaitForInitialization;
            var custom = _configuration.DllBuild is DllBuild.Custom;
            var client = beta ? FlarialClient.Beta : FlarialClient.Release;

            if (!custom && !beta && !registry.Supported)
            {
                switch (await new UnsupportedVersion(Minecraft.Version, registry.Preferred).PromptAsync())
                {
                    case ContentDialogResult.Primary:
                        _rootPage._versionsPageItem.IsSelected = true;
                        _rootPage.Content = _rootPage._versionsPageItem.Tag;
                        break;

                    case ContentDialogResult.Secondary:
                        _rootPage._settingsPageItem.IsSelected = true;
                        _rootPage.Content = _rootPage._settingsPageItem.Tag;
                        break;
                }
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

                SetVisibility(false);
                _statusTextBlock.Text = "Launching...";

                if (await Task.Run(() => Injector.Launch(initialized, library)) is null)
                {
                    await _launchFailure.ShowAsync();
                    return;
                }

                return;
            }

            if (beta && !await _betaDllEnabled.ShowAsync())
                return;

            SetVisibility(false);
            _statusTextBlock.Text = "Verifying...";

            if (!await client.DownloadAsync(OnFlarialClientDownloadAsync))
            {
                await _clientUpdateFailure.ShowAsync();
                return;
            }

            _progressBar.IsIndeterminate = true;
            _statusTextBlock.Text = "Launching...";

            if (!await Task.Run(() => client.Launch(initialized)))
            {
                await _launchFailure.ShowAsync();
                return;
            }
        }
        finally
        {
            SetVisibility(true);
        }
    }

    internal HomePage(RootPage rootPage, Configuration configuration, WindowInteropHelper helper)
    {
        _helper = helper;
        _rootPage = rootPage;
        _configuration = configuration;

        Children.Add(_launcherVersionTextBlock);
        Children.Add(_packageVersionTextBlock);

        Children.Add(_logoImage);
        Children.Add(_progressBar);
        Children.Add(_statusTextBlock);
        Children.Add(_playButton);

        Children.Add(_leftSponsorshipImage);
        Children.Add(_centerSponsorshipImage);
        Children.Add(_rightSponsorshipImage);

        _playButton.Click += OnPlayButtonClick;
        _leftSponsorshipImage.MouseLeftButtonDown += OnSponsorshipImageClick;
        _centerSponsorshipImage.MouseLeftButtonDown += OnSponsorshipImageClick;
        _rightSponsorshipImage.MouseLeftButtonDown += OnSponsorshipImageClick;
    }
}
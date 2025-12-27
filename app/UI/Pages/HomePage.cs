using System.Windows;
using System.Windows.Media;
using Flarial.Launcher.App;
using ModernWpf.Controls;
using System.Windows.Controls;
using Flarial.Launcher.Services.SDK;
using Flarial.Launcher.Services.Management.Versions;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Core;
using System;
using Flarial.Launcher.Services.Client;
using Flarial.Launcher.Services.Modding;
using System.Windows.Threading;

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

    readonly TextBlock _textBlock = new()
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
        Content = new SymbolIcon(Symbol.Play),
        Width = ApplicationManifest.Icon.Width * 2,
        Margin = new(0, 120, 0, 0)
    };

    internal HomePage(Configuration configuration, VersionCatalog catalog, Image? banner)
    {
        Children.Add(_logo);
        Children.Add(_progressBar);
        Children.Add(_textBlock);
        Children.Add(_button);
        if (banner is { }) Children.Add(banner);

        _button.Click += async (_, _) =>
        {
            try
            {
                _button.Visibility = Visibility.Hidden;

                _progressBar.IsIndeterminate = true;
                _progressBar.Visibility = Visibility.Visible;

                _textBlock.Visibility = Visibility.Visible;

                if (!Minecraft.IsInstalled)
                {
                    await MessageDialog.ShowAsync(MessageDialogContent._notInstalled);
                    return;
                }

                if (!Minecraft.IsSigned)
                {
                    await MessageDialog.ShowAsync(MessageDialogContent._notSigned);
                    return;
                }

                var beta = configuration.DllBuild is DllBuild.Beta;
                var custom = configuration.DllBuild is DllBuild.Custom;

                beta = beta || Minecraft.UsingGameDevelopmentKit;
                var client = beta ? FlarialClient.Beta : FlarialClient.Release;

                if (!custom && !beta && !catalog.IsSupported)
                {
                    await MessageDialog.ShowAsync(MessageDialogContent._unsupportedVersion);
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

                    _textBlock.Text = "Launching...";

                    if (await Task.Run(() => Injector.Launch(configuration.WaitForInitialization, library)) is null)
                    {
                        await MessageDialog.ShowAsync(MessageDialogContent._launchFailure);
                        return;
                    }

                    return;
                }

                _textBlock.Text = "Verifying...";

                if (!await client.DownloadAsync(_ => Dispatcher.Invoke(() =>
                {
                    if (_progressBar.Value == _) return;

                    _textBlock.Text = "Downloading...";

                    _progressBar.Value = _;
                    _progressBar.IsIndeterminate = false;
                })))
                {
                    await MessageDialog.ShowAsync(MessageDialogContent._updateFailure);
                    return;
                }

                _textBlock.Text = "Launching...";
                _progressBar.IsIndeterminate = true;

                if (beta && await MessageDialog.ShowAsync(MessageDialogContent._betaUsage))
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

                _textBlock.Text = "Preparing...";
                _textBlock.Visibility = Visibility.Hidden;

                _button.Visibility = Visibility.Visible;
            }
        };
    }
}
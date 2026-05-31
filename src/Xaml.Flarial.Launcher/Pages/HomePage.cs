using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using Flarial.Launcher.Controls;
using Flarial.Launcher.Interface.Dialogs;
using Flarial.Launcher.Interface.Presentation;
using Flarial.Launcher.Management;
using Flarial.Launcher.Xaml;
using Flarial.Runtime.Analytics;
using Flarial.Runtime.Core;
using Flarial.Runtime.Game;
using Flarial.Runtime.Modding;
using Flarial.Runtime.Versions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Flarial.Launcher.Pages;

sealed class HomePage : Grid
{
    readonly Image _image = new()
    {
        Width = 96,
        Height = 96,
        Margin = new(0, 0, 0, 90),
        Source = new BitmapImage(),
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center
    };

    internal readonly Button _button = new()
    {
        Content = "Connecting...",
        Width = 256,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new(0, 90, 0, 0),
        IsEnabled = false
    };

    internal readonly TextBlock _leftText = new()
    {
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalAlignment = HorizontalAlignment.Left,
        Margin = new(12, 12, 0, 0),
        Text = "⚪ 0.0.0"
    };

    readonly TextBlock _rightText = new()
    {
        Margin = new(0, 12, 12, 0),
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalAlignment = HorizontalAlignment.Right,
        Text = AppManifest.s_version
    };

    readonly AppContent _content;
    readonly AppSettings _settings;

    UnsupportedVersionDialog UnsupportedVersion
    {
        get
        {
            if (field is null)
            {
                var registry = (VersionRegistry)Tag;
                field = new(registry.PreferredVersion);
            }
            return field;
        }
    }

    internal HomePage(AppContent content, AppSettings settings)
    {
        _content = content;
        _settings = settings;

        using (var stream = AppManifest.GetStream("Application.ico"))
        {
            var source = (BitmapImage)_image.Source;
            source.SetSource(stream.AsRandomAccessStream());
        }

        Children.Add(_leftText);
        Children.Add(_rightText);

        Children.Add(_button);
        Children.Add(_image);

        Children.Add(new PromotionImagesBox
        {
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        _button.Click += OnButtonClick;
    }

    void OnFlarialClientDownloadAsync(int value) => Dispatcher.Invoke(() => _button.Content = $"Downloading... {value}%");

    async void OnButtonClick(object sender, RoutedEventArgs args)
    {
        var button = (Button)sender; try
        {
            button.IsEnabled = false;
            button.Content = "Play";

            var path = _settings.CustomDllPath;
            var registry = (VersionRegistry)Tag;
            var custom = _settings.UseCustomDll;

            if (!Minecraft.IsInstalled)
            {
                await NotInstalledDialog.ShowAsync();
                return;
            }

            if (!Minecraft.IsGamingServicesInstalled)
            {
                await GamingServicesMissingDialog.ShowAsync();
                return;
            }

            if (!custom && !registry.IsSupported && await UnsupportedVersion.OnShowAsync())
            {
                (~_content).SelectedItem = _content._versionsItem;
                (~_content).Content = _content._versionsItem.Tag;
                return;
            }

            if (custom)
            {
                Library library = new(path);

                if (!library.IsLoadable)
                {
                    await InvalidCustomDllDialog.ShowAsync();
                    return;
                }

                _button.Content = "Launching...";
                if (await Injector.LaunchAsync(library) is null)
                {
                    await LaunchFailureDialog.ShowAsync();
                    return;
                }

                return;
            }

            _button.Content = "Verifying...";
            if (!await FlarialClient.Current.DownloadAsync(OnFlarialClientDownloadAsync))
            {
                await ClientUpdateFailureDialog.ShowAsync();
                return;
            }

            _button.Content = "Launching...";
            if (!await FlarialClient.Current.TrackedLaunchAsync() ?? false)
            {
                await LaunchFailureDialog.ShowAsync();
                return;
            }
        }
        finally
        {
            button.IsEnabled = true;
            button.Content = "Play";
        }
    }
}
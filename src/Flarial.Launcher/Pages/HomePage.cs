using System.IO;
using System.Threading.Tasks;
using Flarial.Launcher.Controls;
using Flarial.Launcher.Interface;
using Flarial.Launcher.Interface.Dialogs;
using Flarial.Launcher.Management;
using Flarial.Launcher.Runtime.Client;
using Flarial.Launcher.Runtime.Game;
using Flarial.Launcher.Runtime.Modding;
using Flarial.Launcher.Runtime.Versions;
using Flarial.Launcher.Xaml;
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
        Text = ApplicationManifest.s_version
    };

    readonly MainNavigationView _view;
    readonly ApplicationSettings _settings;

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

    internal HomePage(MainNavigationView view, ApplicationSettings settings)
    {
        _view = view;
        _settings = settings;

        using (var stream = ApplicationManifest.GetResourceStream("Application.ico"))
        {
            var source = (BitmapImage)_image.Source;
            source.SetSource(stream.AsRandomAccessStream());
        }

        Children.Add(_leftText);
        Children.Add(_rightText);

        Children.Add(_button);
        Children.Add(_image);

        Children.Add(new PromotionImagesBox()
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
            var initialized = _settings.WaitForInitialization;

            if (!Minecraft.IsInstalled)
            {
                await MainDialog.NotInstalled.ShowAsync();
                return;
            }

            if (!Minecraft.IsGamingServicesInstalled)
            {
                await MainDialog.GamingServicesMissing.ShowAsync();
                return;
            }

            if (!Minecraft.IsPackaged)
            {
                if (!await MainDialog.UnsignedInstall.ShowAsync())
                    return;
            }

            if (!custom && !registry.IsSupported)
            {
                switch (await UnsupportedVersion.PromptAsync())
                {
                    case ContentDialogResult.Primary:
                        (~_view).SelectedItem = _view._versionsItem;
                        (~_view).Content = _view._versionsItem.Tag;
                        break;

                    case ContentDialogResult.Secondary:
                        var settingsItem = (NavigationViewItem)(~_view).SettingsItem;
                        (~_view).SelectedItem = settingsItem;
                        (~_view).Content = settingsItem.Tag;
                        break;
                }
                return;
            }

            if (custom)
            {
                if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
                {
                    await MainDialog.InvalidCustomDll.ShowAsync();
                    return;
                }

                Library library = new(path); if (!library.IsLoadable)
                {
                    await MainDialog.InvalidCustomDll.ShowAsync();
                    return;
                }

                _button.Content = "Launching...";
                if (await Task.Run(() => Injector.Launch(initialized, library)) is null)
                {
                    await MainDialog.LaunchFailure.ShowAsync();
                    return;
                }

                return;
            }

            _button.Content = "Verifying...";
            if (!await FlarialClient.Current.DownloadAsync(OnFlarialClientDownloadAsync))
            {
                await MainDialog.ClientUpdateFailure.ShowAsync();
                return;
            }

            _button.Content = "Launching...";
            if (!await Task.Run(() => FlarialClient.Current.Launch(initialized)) ?? false)
                await MainDialog.LaunchFailure.ShowAsync();
            else
                await MainDialog.ClientInjectionFailure.ShowAsync();
        }
        finally
        {
            button.IsEnabled = true;
            button.Content = "Play";
        }
    }
}
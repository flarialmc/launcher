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
using Windows.Phone.Notification.Management;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Flarial.Launcher.Pages;

sealed class HomePage : XamlElement<Grid>
{
    readonly Image _logoImage = new()
    {
        Source = new BitmapImage
        {
            DecodePixelType = DecodePixelType.Logical,
            DecodePixelWidth = 96,
            DecodePixelHeight = 96
        },
        Stretch = Stretch.None,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new(0, 0, 0, 90)
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
        Text = "❌ 0.0.0"
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

    readonly PromotionImageButton _leftImageButton = new LiteByteHostingImageButton();
    readonly PromotionImageButton _rightImageButton = new CollapseNetworkImageButton();
    readonly PromotionImageButton _centerImageButton = new InfinityNetworkImageButton();

    UnsupportedVersionDialog UnsupportedVersion
    {
        get
        {
            if (field is null)
            {
                var registry = (VersionRegistry)@this.Tag;
                field = new(registry.PreferredVersion);
            }
            return field;
        }
    }

    internal HomePage(MainNavigationView view, ApplicationSettings settings) : base(new())
    {
        _view = view;
        _settings = settings;

        using (var stream = ApplicationManifest.GetResourceStream("Application.ico"))
        {
            var logoBitmap = (BitmapImage)_logoImage.Source;
            logoBitmap.SetSource(stream.AsRandomAccessStream());
        }

        _leftImageButton.@this.Margin = new(12, 0, 0, 12);
        _leftImageButton.@this.HorizontalAlignment = HorizontalAlignment.Left;

        _centerImageButton.@this.Margin = new(0, 0, 0, 12);
        _centerImageButton.@this.HorizontalAlignment = HorizontalAlignment.Center;

        _rightImageButton.@this.Margin = new(0, 0, 12, 12);
        _rightImageButton.@this.HorizontalAlignment = HorizontalAlignment.Right;

        @this.Children.Add(_leftText);
        @this.Children.Add(_rightText);

        @this.Children.Add(_button);
        @this.Children.Add(_logoImage);

        @this.Children.Add(_leftImageButton.@this);
        @this.Children.Add(_centerImageButton.@this);
        @this.Children.Add(_rightImageButton.@this);

        _button.Click += OnButtonClick;
    }

    void OnFlarialClientDownloadAsync(int value) => @this.Dispatcher.Invoke(() =>
    {
        _button.Content = $"Downloading... {value}%";
    });

    async void OnButtonClick(object sender, RoutedEventArgs args)
    {
        var button = (Button)sender; try
        {
            button.IsEnabled = false;
            button.Content = "Play";

            var beta = _settings.DllSelection is DllSelection.Beta;
            var custom = _settings.DllSelection is DllSelection.Custom;

            var path = _settings.CustomDllPath;
            var initialized = _settings.WaitForInitialization;

            var registry = (VersionRegistry)@this.Tag;
            var client = beta ? FlarialClient.Beta : FlarialClient.Release;

            if (!Minecraft.IsInstalled)
            {
                await MainDialog.NotInstalled.ShowAsync(@this);
                return;
            }

            if (Minecraft.UsingGameDevelopmentKit)
            {
                if (!Minecraft.IsGamingServicesInstalled)
                {
                    await MainDialog.GamingServicesMissing.ShowAsync(@this);
                    return;
                }

                if (!Minecraft.IsPackaged)
                {
                    if ((bool)Minecraft.AllowUnsignedInstalls!)
                    {
                        if (!await MainDialog.AllowUnsignedInstall.ShowAsync(@this))
                            return;
                    }
                    else
                    {
                        await MainDialog.UnsignedInstall.ShowAsync(@this);
                        return;
                    }
                }
            }
            else
            {
                await MainDialog.UWPDeprecated.ShowAsync(@this);
                return;
            }

            if (!custom && !beta && !registry.IsSupported)
            {
                switch (await UnsupportedVersion.PromptAsync(@this))
                {
                    case ContentDialogResult.Primary:
                        _view.@this.SelectedItem = _view._versionsItem;
                        _view.@this.Content = _view._versionsPage.@this;
                        break;

                    case ContentDialogResult.Secondary:
                        _view.@this.Content = _view._settingsPage.@this;
                        _view.@this.SelectedItem = (NavigationViewItem)_view.@this.SettingsItem;
                        break;
                }
                return;
            }

            if (custom)
            {
                if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
                {
                    await MainDialog.InvalidCustomDll.ShowAsync(@this);
                    return;
                }

                Library library = new(path);

                if (!library.IsLoadable)
                {
                    await MainDialog.InvalidCustomDll.ShowAsync(@this);
                    return;
                }

                _button.Content = "Launching...";
                if (await Task.Run(() => Injector.Launch(initialized, library)) is null)
                {
                    await MainDialog.LaunchFailure.ShowAsync(@this);
                    return;
                }

                return;
            }


            if (beta && !await MainDialog.BetaDllUsage.ShowAsync(@this))
                return;

            _button.Content = "Verifying...";
            if (!await client.DownloadAsync(OnFlarialClientDownloadAsync))
            {
                await MainDialog.ClientUpdateFailure.ShowAsync(@this);
                return;
            }

            _button.Content = "Launching...";
            if (!await Task.Run(() => client.Launch(initialized)))
            {
                await MainDialog.LaunchFailure.ShowAsync(@this);
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
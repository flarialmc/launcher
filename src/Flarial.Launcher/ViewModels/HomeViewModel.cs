using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using Avalonia.Media;
using Flarial.Launcher.Management;
using Flarial.Launcher.Services;
using Flarial.Launcher.Types;
using Flarial.Runtime.Analytics;
using Flarial.Runtime.Core;
using Flarial.Runtime.Game;
using Flarial.Runtime.Modding;
using Flarial.Runtime.Versions;

namespace Flarial.Launcher.ViewModels;

public class HomeViewModel : ViewModelBase
{
    readonly AppSettings _settings;
    readonly IDialogService _dialogService;
    readonly INotificationService _notificationService;
    VersionRegistry? _versionRegistry;

    public string MinecraftVersion
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = "0.0.0";

    public IBrush MinecraftVersionBackground
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = Brushes.Gray;

    public string MinecraftVersionTooltip
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = "Minecraft is not installed.";

    public string LauncherVersion { get; } = LauncherInfo.Version;

    public string Status
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "Connecting...";

    public string LaunchText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "Connecting...";

    public bool IsLaunchEnabled
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            _launch?.RaiseCanExecuteChanged();
        }
    }

    readonly AsyncRelayCommand _launch;

    public ICommand Launch => _launch;

    public HomeViewModel(AppSettings settings, IDialogService dialogService, INotificationService notificationService)
    {
        _settings = settings;
        _dialogService = dialogService;
        _notificationService = notificationService;

        _launch = new AsyncRelayCommand(LaunchAsync, () => IsLaunchEnabled);
    }

    public void SetVersionRegistry(VersionRegistry registry) => _versionRegistry = registry;

    public void UpdateMinecraftStatus()
    {
        if (!Minecraft.IsInstalled)
        {
            MinecraftVersion = "0.0.0";
            MinecraftVersionBackground = Brushes.Gray;
            MinecraftVersionTooltip = "Minecraft is not installed.";
            return;
        }

        var supported = _versionRegistry?.IsSupported ?? false;
        MinecraftVersion = $"{(supported ? "Supported," : "Unsupported,")} {VersionRegistry.InstalledVersion}";
        MinecraftVersionBackground = supported ? Brushes.ForestGreen : Brushes.DarkRed;
        MinecraftVersionTooltip = supported
            ? "Your Minecraft version is supported by Flarial."
            : "Your Minecraft version is unsupported by Flarial.";
    }

    async Task LaunchAsync()
    {
        try
        {
            IsLaunchEnabled = false;
            LaunchText = "Launch";

            if (!Minecraft.IsInstalled)
            {
                await _dialogService.ShowMessageBoxAsync("Minecraft not installed", "Minecraft for Windows is not installed.", ["OK"]);
                return;
            }

            if (!Minecraft.IsGamingServicesInstalled)
            {
                await _dialogService.ShowMessageBoxAsync("Gaming Services missing", "Microsoft Gaming Services is required to launch Minecraft.", ["OK"]);
                return;
            }

            if (!_settings.UseCustomDll && _versionRegistry is { IsSupported: false })
            {
                var result = await _dialogService.ShowMessageBoxAsync(
                    "Unsupported version",
                    $"Your current Minecraft version is not supported. The preferred supported version is {_versionRegistry.PreferredVersion}.",
                    ["Versions", "Custom DLL", "Cancel"]);

                if (result == "Versions")
                    NavigateToSettings(PageTransitions.SettingsVersionsPage);
                else if (result == "Custom DLL")
                    NavigateToSettings(PageTransitions.SettingsGeneralPage);

                return;
            }

            if (_settings.UseCustomDll)
            {
                await LaunchCustomDllAsync();
                return;
            }

            LaunchText = "Verifying...";
            if (!await FlarialClient.Current.DownloadAsync(value => Dispatcher.UIThread.Post(() => LaunchText = $"Downloading... {value}%")))
            {
                await _dialogService.ShowMessageBoxAsync("Client update failed", "Unable to download the latest Flarial client.", ["OK"]);
                return;
            }

            LaunchText = "Launching...";
            if (!await FlarialClient.Current.TrackedLaunchAsync() ?? false)
            {
                await _dialogService.ShowMessageBoxAsync("Launch failed", "Unable to launch and inject Flarial.", ["OK"]);
            }
        }
        catch (Exception exception)
        {
            var message = exception.GetBaseException().Message;
            await _dialogService.ShowMessageBoxAsync("Launch failed", message, ["OK"]);
        }
        finally
        {
            IsLaunchEnabled = true;
            LaunchText = "Launch";
        }
    }

    async Task LaunchCustomDllAsync()
    {
        var path = _settings.CustomDllPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            await _dialogService.ShowMessageBoxAsync("Invalid DLL", "Select a valid custom DLL before launching.", ["OK"]);
            return;
        }

        Library library = new(path);
        if (!library.IsLoadable)
        {
            await _dialogService.ShowMessageBoxAsync("Invalid DLL", $"{Path.GetFileName(path)} could not be loaded.", ["OK"]);
            return;
        }

        LaunchText = "Launching...";
        if (await Task.Run(() => Injector.Launch(library)) is null)
        {
            await _dialogService.ShowMessageBoxAsync("Launch failed", "Unable to launch and inject the selected DLL.", ["OK"]);
        }
    }

    static void NavigateToSettings(PageTransitions page)
    {
        AppMessageBus.Send(PageTransitions.SettingsPage);
        AppMessageBus.Send(page);
    }

    public ICommand MinimizeWindow { get; } =
        new RelayCommand(() => AppMessageBus.Send(WindowStateArgs.Minimize));

    public ICommand CloseWindow { get; } =
        new RelayCommand(() => AppMessageBus.Send(WindowStateArgs.Close));
}

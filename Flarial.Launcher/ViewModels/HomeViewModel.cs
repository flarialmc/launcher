using System;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Threading;
using Flarial.Launcher.Management;
using Flarial.Launcher.Services;
using Flarial.Launcher.Types;
using Flarial.Runtime.Analytics;
using Flarial.Runtime.Core;
using Flarial.Runtime.Game;
using Flarial.Runtime.Modding;
using Flarial.Runtime.Versions;
using ReactiveUI;

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
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ReactiveCommand<Unit, Unit> Launch { get; }

    public HomeViewModel(AppSettings settings, IDialogService dialogService, INotificationService notificationService)
    {
        _settings = settings;
        _dialogService = dialogService;
        _notificationService = notificationService;

        Launch = ReactiveCommand.CreateFromTask(LaunchAsync, this.WhenAnyValue(x => x.IsLaunchEnabled));
    }

    public void SetVersionRegistry(VersionRegistry registry) => _versionRegistry = registry;

    public void UpdateMinecraftStatus()
    {
        if (!Minecraft.IsInstalled)
        {
            MinecraftVersion = "0.0.0";
            return;
        }

        var supported = _versionRegistry?.IsSupported ?? false;
        MinecraftVersion = $"{(supported ? "Supported" : "Unsupported")} {VersionRegistry.InstalledVersion}";
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
        MessageBus.Current.SendMessage(PageTransitions.SettingsPage);
        MessageBus.Current.SendMessage(page);
    }

    public ReactiveCommand<Unit, Unit> MinimizeWindow { get; } =
        ReactiveCommand.Create(() => MessageBus.Current.SendMessage(WindowStateArgs.Minimize));

    public ReactiveCommand<Unit, Unit> CloseWindow { get; } =
        ReactiveCommand.Create(() => MessageBus.Current.SendMessage(WindowStateArgs.Close));
}

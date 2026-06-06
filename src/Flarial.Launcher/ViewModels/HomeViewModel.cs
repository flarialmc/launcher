using System;
using System.Reactive;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using Flarial.Launcher.Dialogs;
using Flarial.Launcher.Dialogs.Metadata;
using Flarial.Launcher.Management;
using Flarial.Launcher.Types;
using Flarial.Runtime.Analytics;
using Flarial.Runtime.Core;
using Flarial.Runtime.Game;
using Flarial.Runtime.Versions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    [Reactive] bool _isLaunching = true;

    [Reactive] string _launcherVersion;
    [Reactive] string _launcherStatus = "Preparing...";

    [Reactive] string _gameVersion = "0.0.0";
    [Reactive] IImmutableSolidColorBrush _gameVersionColor = Brushes.Gray;

    UnsupportedVersionDialog UnsupportedVersionDialog => field ??= new(_mainWindowViewModel.VersionRegistry.PreferredVersion);

    readonly AppSettings _appSettings;
    readonly MainWindowViewModel _mainWindowViewModel;

    public HomeViewModel(MainWindowViewModel mainWindowViewModel, AppSettings appSettings)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _appSettings = appSettings;

        var assembly = Assembly.GetExecutingAssembly();
        _launcherVersion = $"{assembly.GetName().Version}";

        Launch = ReactiveCommand.CreateFromTask(OnLaunchAsync);
        CloseWindow = ReactiveCommand.Create(() => MessageBus.Current.SendMessage(WindowStateArgs.Close));
        MinimizeWindow = ReactiveCommand.Create(() => MessageBus.Current.SendMessage(WindowStateArgs.Minimize));
    }

    async Task OnLaunchAsync()
    {
        IsLaunching = true;
        try
        {
            var path = _appSettings.CustomDllPath;
            var custom = _appSettings.UseCustomDll;
            var compatible = _appSettings.CompatibilityMode;

            if (!GamingServices.IsInstalled)
            {
                await GamingServicesMissingDialog._.ShowAsync();
                return;
            }

            if (!Minecraft.IsInstalled)
            {
                await NotInstalledDialog._.ShowAsync();
                return;
            }

            if (compatible && Minecraft.IsSideloaded && !Minecraft.IsRunning)
            {
                await CompatibilityModeDialog._.ShowAsync();
                return;
            }

            if (!custom && !_mainWindowViewModel.VersionRegistry.IsSupported)
            {
                await UnsupportedVersionDialog.ShowAsync();
                return;
            }

            if (custom)
            {
                Library library = new(path); if (!library.IsLoadable)
                {
                    await InvalidCustomDllDialog._.ShowAsync();
                    return;
                }

                LauncherStatus = "Launching...";
                if (await Task.Run(() => Injector.Launch(compatible, library)) is null)
                {
                    await LaunchFailureDialog._.ShowAsync();
                    return;
                }

                return;
            }

            LauncherStatus = "Verifying...";
            if (!await FlarialClient.DownloadAsync(OnDownload))
            {
                await ClientUpdateFailureDialog._.ShowAsync();
                return;
            }

            LauncherStatus = "Launching...";
            if (!await FlarialClient.TrackedLaunchAsync(compatible) ?? false)
            {
                await LaunchFailureDialog._.ShowAsync();
                return;
            }
        }
        finally
        {
            IsLaunching = false;
            LauncherStatus = "Ready!";
        }
    }

    void OnDownload(int value) => LauncherStatus = $"Downloading... {value}%";

    public void OnPackageStatusChanged()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Invoke(OnPackageStatusChanged);
            return;
        }

        if (!Minecraft.IsInstalled)
        {
            GameVersion = "0.0.0";
            GameVersionColor = Brushes.Gray;
            return;
        }

        GameVersion = VersionRegistry.InstalledVersion;
        GameVersionColor = _mainWindowViewModel.VersionRegistry.IsSupported ? Brushes.DarkGreen : Brushes.DarkRed;
    }

    public ReactiveCommand<Unit, Unit> Launch { get; }
    public ReactiveCommand<Unit, Unit> CloseWindow { get; }
    public ReactiveCommand<Unit, Unit> MinimizeWindow { get; }
}
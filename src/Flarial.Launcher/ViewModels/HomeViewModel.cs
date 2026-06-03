using System;
using System.Reactive;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
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
    [Reactive] bool _isLaunching;
    [Reactive] bool _isInitialized;

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

            if (!Minecraft.IsGamingServicesInstalled)
            {
                await GamingServicesMissingDialog.ShowAsync();
                return;
            }

            if (!Minecraft.IsInstalled)
            {
                await NotInstalledDialog.ShowAsync();
                return;
            }

            if (!custom && !_mainWindowViewModel.VersionRegistry.IsSupported)
            {
                await UnsupportedVersionDialog.OnShowAsync();
                return;
            }

            if (custom)
            {
                Library library = new(path); if (!library.IsLoadable)
                {
                    await InvalidCustomDllDialog.ShowAsync();
                    return;
                }

                LauncherStatus = "Launching...";
                if (await Task.Run(() => Injector.Launch(library)) is null)
                {
                    await LaunchFailureDialog.ShowAsync();
                    return;
                }

                return;
            }

            LauncherStatus = "Verifying...";
            if (!await FlarialClient.DownloadAsync(OnDownload))
            {
                await ClientUpdateFailureDialog.ShowAsync();
                return;
            }

            LauncherStatus = "Launching...";
            if (!await FlarialClient.TrackedLaunchAsync() ?? false)
            {
                await LaunchFailureDialog.ShowAsync();
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
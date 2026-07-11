using System.Reactive;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Flarial.Launcher.Dialogs.Metadata;
using Flarial.Launcher.Management;
using Flarial.Launcher.Types;
using Flarial.Runtime.Client;
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

    UnsupportedVersionDialog UnsupportedVersionDialog => field ??= new(_model.VersionRegistry);

    readonly MainWindowViewModel _model;
    readonly AppSettings _settings = ((App)Application.Current!).Settings;

    public HomeViewModel(MainWindowViewModel model)
    {
        _model = model;

        var assembly = Assembly.GetExecutingAssembly();
        _launcherVersion = $"{assembly.GetName().Version}";

        Launch = ReactiveCommand.CreateFromTask(OnLaunchAsync);
        CloseWindow = ReactiveCommand.Create(static () => MessageBus.Current.SendMessage(WindowStateArgs.Close));
        MinimizeWindow = ReactiveCommand.Create(static () => MessageBus.Current.SendMessage(WindowStateArgs.Minimize));
    }

    async Task OnLaunchAsync()
    {
        IsLaunching = true;
        try
        {
            var path = _settings.CustomDllPath;
            var custom = _settings.UseCustomDll;

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

            if (Minecraft.IsSideloaded && !Minecraft.IsRunning)
            {
                if (!await SideloadedBootstrapDialog._.ShowAsync())
                    return;
            }

            if (!custom && !_model.VersionRegistry.IsSupported)
            {
                await UnsupportedVersionDialog.ShowAsync();
                return;
            }

            if (custom)
            {
                Library library = new(path);

                if (!library.IsLoadable)
                {
                    await InvalidCustomDllDialog._.ShowAsync();
                    return;
                }

                LauncherStatus = "Launching...";
                if (!await Task.Run(() => Injector.Launch(library)))
                {
                    await LaunchFailureDialog._.ShowAsync();
                    return;
                }

                return;
            }

            LauncherStatus = "Verifying...";
            if (!await FlarialClientRelease._.DownloadAsync(OnDownload))
            {
                await ClientUpdateFailureDialog._.ShowAsync();
                return;
            }

            if (FlarialClient.IsRunning)
            {
                await ClientAlreadyInjectedDialog._.ShowAsync();
                return;
            }

            LauncherStatus = "Launching...";
            if (!await Task.Run(FlarialClientRelease._.Launch))
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
        if (!Minecraft.IsInstalled)
        {
            GameVersion = "0.0.0";
            GameVersionColor = Brushes.Gray;
            return;
        }

        GameVersion = VersionRegistry.InstalledVersion;
        GameVersionColor = _model.VersionRegistry.IsSupported ? Brushes.DarkGreen : Brushes.DarkRed;
    }

    public ReactiveCommand<Unit, Unit> Launch { get; }
    public ReactiveCommand<Unit, Unit> CloseWindow { get; }
    public ReactiveCommand<Unit, Unit> MinimizeWindow { get; }
}
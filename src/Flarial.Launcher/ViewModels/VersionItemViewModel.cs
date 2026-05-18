using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using Flarial.Launcher.Models;
using Flarial.Runtime.Game;
using Flarial.Runtime.Versions;

namespace Flarial.Launcher.ViewModels;

public class VersionItemViewModel : ViewModelBase
{
    readonly VersionItem _item;
    readonly MainWindowViewModel _host;
    readonly HomeViewModel _home;

    public string Version { get; }

    public VersionItemState State
    {
        get;
        private set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            this.RaisePropertyChanged(nameof(IsNotInstalled));
            this.RaisePropertyChanged(nameof(IsDownloading));
            this.RaisePropertyChanged(nameof(IsInstalled));
            _installCommand?.RaiseCanExecuteChanged();
            _deleteCommand?.RaiseCanExecuteChanged();
        }
    }

    public double InstallPercentage
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    readonly AsyncRelayCommand _installCommand;
    readonly AsyncRelayCommand _deleteCommand;

    public ICommand InstallCommand => _installCommand;
    public ICommand DeleteCommand => _deleteCommand;

    public bool IsNotInstalled => State == VersionItemState.NotInstalled;
    public bool IsDownloading => State == VersionItemState.Downloading;
    public bool IsInstalled => State == VersionItemState.Installed;

    public VersionItemViewModel(VersionItem item, VersionItemState state, MainWindowViewModel host, HomeViewModel home)
    {
        _item = item;
        _host = host;
        _home = home;

        Version = item.ToString();
        State = state;

        _installCommand = new AsyncRelayCommand(
            InstallAsync,
            () => State == VersionItemState.NotInstalled,
            ex => _ = ShowFailureAsync(ex));

        _deleteCommand = new AsyncRelayCommand(
            DeleteAsync,
            () => State == VersionItemState.Installed,
            ex => _ = ShowFailureAsync(ex));
    }

    async Task InstallAsync()
    {
        if (!Minecraft.IsInstalled)
        {
            await _host.ShowMessageBoxAsync("Minecraft not installed", "Minecraft for Windows is not installed.", ["OK"]);
            return;
        }

        if (!Minecraft.IsPackaged)
        {
            await _host.ShowMessageBoxAsync("Unpackaged install", "Version changing requires the packaged Store installation of Minecraft.", ["OK"]);
            return;
        }

        if (!Minecraft.IsGamingServicesInstalled)
        {
            await _host.ShowMessageBoxAsync("Gaming Services missing", "Microsoft Gaming Services is required to install Minecraft versions.", ["OK"]);
            return;
        }

        if (!await _host.ConfirmAsync("Install version", $"Install Minecraft {Version}?", "Install", "Cancel"))
            return;

        State = VersionItemState.Downloading;
        InstallPercentage = 0;
        _host.SetVersionInstallActive(true);

        try
        {
            await _item.InstallAsync((value, _) =>
            {
                Dispatcher.UIThread.Post(() => InstallPercentage = Math.Min(Math.Max(value, 0), 100));
            });

            State = VersionItemState.Installed;
            _home.UpdateMinecraftStatus();
        }
        finally
        {
            _host.SetVersionInstallActive(false);
            InstallPercentage = 0;
            if (State == VersionItemState.Downloading)
                State = VersionItemState.NotInstalled;
        }
    }

    async Task DeleteAsync()
    {
        await _host.ShowMessageBoxAsync("Not available", "The backend does not expose version deletion.", ["OK"]);
    }

    async Task ShowFailureAsync(Exception exception)
    {
        await _host.ShowMessageBoxAsync("Version install failed", exception.Message, ["OK"]);
        State = VersionItemState.NotInstalled;
        InstallPercentage = 0;
    }
}

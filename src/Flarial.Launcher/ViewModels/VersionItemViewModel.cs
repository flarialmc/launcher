using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Flarial.Launcher.Dialogs.Metadata;
using Flarial.Launcher.Models;
using Flarial.Launcher.Views;
using Flarial.Runtime.Game;
using Flarial.Runtime.Versions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Flarial.Launcher.ViewModels;

public partial class VersionItemViewModel : ViewModelBase
{
    public string Version { get; }

    [Reactive]
    VersionItemState _state;

    [Reactive]
    double _installPercentage;

    [Reactive]
    bool _isProgressing;

    public ReactiveCommand<Unit, Unit> InstallCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    public bool IsNotInstalled => State is VersionItemState.NotInstalled;
    public bool IsDownloading => State is VersionItemState.Downloading;
    public bool IsInstalled => State is VersionItemState.Installed;
    public bool IsInstalling => State is VersionItemState.Installing;

    readonly MainWindow _mainWindow;
    readonly VersionItem _versionItem;
    readonly SettingsVersionsViewModel _settingsVersionsViewModel;

    InstallVersionDialog InstallVersionDialog => field ??= new(_versionItem);
    InstalledVersionDialog InstalledVersionDialog => field ??= new(_versionItem);
    InstallingVersionDialog InstallingVersionDialog => field ??= new(_versionItem);

    Task? InstallingVersionDialogTask
    {
        get
        {
            if (field is null) return null;
            if (field.IsCompleted) return null;
            return field;
        }
        set;
    }


    public VersionItemViewModel(MainWindowViewModel mainWindowViewModel, VersionItem versionItem)
    {
        var application = Application.Current!;
        var applicationLifetime = (IClassicDesktopStyleApplicationLifetime)application.ApplicationLifetime!;

        Version = $"{versionItem}";

        _versionItem = versionItem;
        _settingsVersionsViewModel = mainWindowViewModel.SettingsViewModel.SettingsVersionsViewModel;

        _mainWindow = (MainWindow)applicationLifetime.MainWindow!;


        this.WhenAnyValue(static _ => _.State).Subscribe(_ =>
        {
            this.RaisePropertyChanged(nameof(IsNotInstalled));
            this.RaisePropertyChanged(nameof(IsDownloading));
            this.RaisePropertyChanged(nameof(IsInstalled));
            this.RaisePropertyChanged(nameof(IsInstalling));
        });

        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync, this.WhenAnyValue(static _ => _.State).Select(static _ => _ == VersionItemState.Installed));
        InstallCommand = ReactiveCommand.CreateFromTask(InstallAsync, this.WhenAnyValue(static _ => _.State).Select(static _ => _ == VersionItemState.NotInstalled));
    }

    void OnInstall(int percentage, bool installing)
    {
        InstallPercentage = percentage;
        if (installing) State = VersionItemState.Installing;
    }

    async void OnClosing(object? sender, WindowClosingEventArgs args)
    {
        if (!(args.Cancel = IsProgressing)) return;
        if (InstallingVersionDialogTask is { }) return;

        try { await (InstallingVersionDialogTask = InstallingVersionDialog.OnShowAsync()); }
        finally { InstallingVersionDialogTask = null; }
    }

    private async Task InstallAsync()
    {
        if (!Minecraft.IsInstalled)
        {
            await NotInstalledDialog.ShowAsync();
            return;
        }

        if (!GamingServices.IsInstalled)
        {
            await GamingServicesMissingDialog.ShowAsync();
            return;
        }

        if (!Minecraft.IsPackaged)
        {
            await UnpackagedInstallDialog.ShowAsync();
            return;
        }

        if (!await InstallVersionDialog.OnShowAsync())
            return;

        try
        {
            IsProgressing = true;
            _mainWindow.Closing += OnClosing;
            _settingsVersionsViewModel.IsInstalling = true;

            InstallPercentage = 0;
            State = VersionItemState.Downloading;

            await _versionItem.InstallAsync(OnInstall);
        }
        finally
        {
            InstallPercentage = 0;
            State = VersionItemState.NotInstalled;

            IsProgressing = false;
            _mainWindow.Closing += OnClosing;
            _settingsVersionsViewModel.IsInstalling = false;
        }

        await InstalledVersionDialog.OnShowAsync();
    }

    async Task DeleteAsync() { }
}
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
using Flarial.Runtime.Versions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.ViewModels;

public partial class VersionItemViewModel : ViewModelBase
{
    public string Version { get; }

    [Reactive]
    VersionItemState _state;

    [Reactive]
    double _installPercentage;

    public ReactiveCommand<Unit, Unit> InstallCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    public bool IsNotInstalled => State == VersionItemState.NotInstalled;
    public bool IsDownloading => State == VersionItemState.Downloading;
    public bool IsInstalled => State == VersionItemState.Installed;

    readonly MainWindow _mainWindow;
    readonly VersionItem _versionItem;
    readonly SettingsVersionsViewModel _settingsVersionsViewModel;

    public VersionItemViewModel(MainWindowViewModel mainWindowViewModel, VersionItem versionItem)
    {
        var application = Application.Current!;
        var applicationLifetime = (IClassicDesktopStyleApplicationLifetime)application.ApplicationLifetime!;

        Version = $"{versionItem}";

        _versionItem = versionItem;
        _mainWindow = (MainWindow)applicationLifetime.MainWindow!;
        _settingsVersionsViewModel = mainWindowViewModel.SettingsViewModel.SettingsVersionsViewModel;

        _ = _versionItem;

        this.WhenAnyValue(static _ => _.State).Subscribe(_ =>
        {
            this.RaisePropertyChanged(nameof(IsNotInstalled));
            this.RaisePropertyChanged(nameof(IsDownloading));
            this.RaisePropertyChanged(nameof(IsInstalled));
        });

        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync, this.WhenAnyValue(static _ => _.State).Select(static _ => _ == VersionItemState.Installed));
        InstallCommand = ReactiveCommand.CreateFromTask(InstallAsync, this.WhenAnyValue(static _ => _.State).Select(static _ => _ == VersionItemState.NotInstalled));

        DeleteCommand.ThrownExceptions.Subscribe(static _ => throw _);
        InstallCommand.ThrownExceptions.Subscribe(static _ => throw _);
    }

    async void OnInstall(int value, bool state)
    {
        State = VersionItemState.Downloading;
        InstallPercentage = value;
    }

    async void OnClosing(object sender, WindowClosingEventArgs args)
    {
        if (State != VersionItemState.NotInstalled)
        {
            args.Cancel = true;
            await ExampleDialog.ShowAsync();
        }
    }

    private async Task InstallAsync()
    {
        _mainWindow.Closing += OnClosing;
        _settingsVersionsViewModel.IsInstalling = true;

        try
        {
            State = VersionItemState.Downloading;

            while (InstallPercentage < 100)
            {
                InstallPercentage += 1;
                await Task.Delay(25);
            }

            throw new();
        }
        finally
        {
            InstallPercentage = 0;
            State = VersionItemState.NotInstalled;

            _mainWindow.Closing -= OnClosing;
            _settingsVersionsViewModel.IsInstalling = false;
        }
    }

    async Task DeleteAsync() { }
}
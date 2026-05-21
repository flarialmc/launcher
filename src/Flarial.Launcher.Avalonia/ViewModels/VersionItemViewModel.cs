using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Flarial.Launcher.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.ViewModels;

public partial class VersionItemViewModel : ViewModelBase
{

    public string Version { get; }

    [Reactive]
    private VersionItemState _state;

    [Reactive]
    private double _installPercentage;

    public ReactiveCommand<Unit, Unit> InstallCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    public bool IsNotInstalled => State == VersionItemState.NotInstalled;
    public bool IsDownloading => State == VersionItemState.Downloading;
    public bool IsInstalled => State == VersionItemState.Installed;

    public VersionItemViewModel(VersionItemData data)
    {
        Version = data.Version;
        _state = data.State;

        this.WhenAnyValue(x => x.State)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(IsNotInstalled));
                this.RaisePropertyChanged(nameof(IsDownloading));
                this.RaisePropertyChanged(nameof(IsInstalled));
            });



        InstallCommand = ReactiveCommand.CreateFromTask(
            InstallAsync,
            this.WhenAnyValue(x => x.State).Select(x => x == VersionItemState.NotInstalled)
        );

        InstallCommand.ThrownExceptions
            .Subscribe(ex => throw ex);

        DeleteCommand = ReactiveCommand.CreateFromTask(
            DeleteAsync,
            this.WhenAnyValue(x => x.State).Select(x => x == VersionItemState.Installed)
        );

        DeleteCommand.ThrownExceptions
            .Subscribe(ex => throw ex);
    }

    private async Task InstallAsync()
    {
        State = VersionItemState.Downloading;

        while (InstallPercentage < 100)
        {
            InstallPercentage += 1;
            await Task.Delay(20);
        }

        State = VersionItemState.Installed;
        InstallPercentage = 0;
    }

    private async Task DeleteAsync()
    {
        State = VersionItemState.NotInstalled;
    }
}
using System;
using System.Collections.ObjectModel;
using Flarial.Launcher.Models;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.ViewModels;

public partial class SettingsVersionsViewModel : ViewModelBase
{
    [Reactive]
    bool _isInstalling;

    public ObservableCollection<VersionItemViewModel> Versions { get; } = [];
}
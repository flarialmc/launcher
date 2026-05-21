using System;
using System.Collections.ObjectModel;
using Flarial.Launcher.Models;

namespace Flarial.Launcher.ViewModels;

public class SettingsVersionsViewModel : ViewModelBase
{
    public ReadOnlyObservableCollection<VersionItemViewModel> Versions { get; }

    public SettingsVersionsViewModel()
    {
        ObservableCollection<VersionItemViewModel> source = [new(new("0.0.0", VersionItemState.Installed))];
        Versions = new ReadOnlyObservableCollection<VersionItemViewModel>(source);
    }
}
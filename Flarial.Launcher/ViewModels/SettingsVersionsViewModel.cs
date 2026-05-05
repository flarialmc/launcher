using System;
using System.Collections.ObjectModel;
using Flarial.Launcher.Models;

namespace Flarial.Launcher.ViewModels;

public class SettingsVersionsViewModel : ViewModelBase
{
    public ReadOnlyObservableCollection<VersionItemViewModel> Versions { get; }
    
    public SettingsVersionsViewModel()
    {
        var source = new ObservableCollection<VersionItemViewModel>
        {
            new(new VersionItemData("1.21.132", VersionItemState.NotInstalled)),
            new(new VersionItemData("1.21.130", VersionItemState.Installed)),
            new(new VersionItemData("1.21.121", VersionItemState.Installed)),
            new(new VersionItemData("1.21.114", VersionItemState.NotInstalled)),
            new(new VersionItemData("1.21.110", VersionItemState.NotInstalled)),
            new(new VersionItemData("1.21.110", VersionItemState.NotInstalled)),
            new(new VersionItemData("1.21.110", VersionItemState.NotInstalled)),
            new(new VersionItemData("1.21.110", VersionItemState.NotInstalled)),
            new(new VersionItemData("1.21.110", VersionItemState.NotInstalled)),
            new(new VersionItemData("1.21.110", VersionItemState.NotInstalled)),
            new(new VersionItemData("1.21.110", VersionItemState.NotInstalled)),
            new(new VersionItemData("1.21.110", VersionItemState.NotInstalled)),
            new(new VersionItemData("1.21.110", VersionItemState.NotInstalled)),
            new(new VersionItemData("1.21.110", VersionItemState.NotInstalled)),
            new(new VersionItemData("1.21.110", VersionItemState.NotInstalled)),
            new(new VersionItemData("1.21.110", VersionItemState.NotInstalled)),
        };

        Versions = new ReadOnlyObservableCollection<VersionItemViewModel>(source);
    }
}
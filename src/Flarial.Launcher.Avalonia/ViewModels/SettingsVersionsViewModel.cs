using System;
using System.Collections.ObjectModel;
using Flarial.Launcher.Models;

namespace Flarial.Launcher.ViewModels;

public class SettingsVersionsViewModel : ViewModelBase
{
    public ObservableCollection<VersionItemViewModel> Versions { get; } = [];
}
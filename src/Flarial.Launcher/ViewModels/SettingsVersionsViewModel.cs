using System.Collections.ObjectModel;
using Flarial.Launcher.Models;
using Flarial.Runtime.Game;
using Flarial.Runtime.Versions;

namespace Flarial.Launcher.ViewModels;

public class SettingsVersionsViewModel : ViewModelBase
{
    readonly ObservableCollection<VersionItemViewModel> _source = [];
    readonly MainWindowViewModel _host;
    readonly HomeViewModel _home;

    public ReadOnlyObservableCollection<VersionItemViewModel> Versions { get; }

    public SettingsVersionsViewModel(MainWindowViewModel host, HomeViewModel home)
    {
        _host = host;
        _home = home;
        Versions = new(_source);
    }

    public void SetVersionRegistry(VersionRegistry registry)
    {
        _source.Clear();

        foreach (var item in registry)
        {
            var state = Minecraft.IsInstalled && item.ToString() == VersionRegistry.InstalledVersion
                ? VersionItemState.Installed
                : VersionItemState.NotInstalled;

            _source.Add(new(item, state, _host, _home));
        }
    }
}

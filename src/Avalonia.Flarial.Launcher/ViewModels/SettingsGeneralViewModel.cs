using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Flarial.Launcher.Controls.SegmentedBar;
using Flarial.Launcher.Management;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.ViewModels;

public partial class SettingsGeneralViewModel : ViewModelBase
{
    [Reactive]
    private bool _customDllSelected = true;

    public ObservableCollection<SegmentItem> BuildTypes { get; }

    public SegmentItem? SelectedBuild
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            OnBuildChanged(field);
        }
    }

    public bool AutomaticUpdates
    {
        get;
        set
        {
            _appSettings.AutomaticUpdates = value;
            this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    readonly AppSettings _appSettings;

    public SettingsGeneralViewModel(AppSettings appSettings)
    {
        _appSettings = appSettings;

        BuildTypes =
        [
            new SegmentItem { Title = "Release", Tag = new StrongBox<bool>(false) },
            /* new SegmentItem { Title = "Custom", Tag = new StrongBox<bool>(true)} */
        ];

        SelectedBuild = BuildTypes.FirstOrDefault();
        AutomaticUpdates = _appSettings.AutomaticUpdates;
    }

    private void OnBuildChanged(SegmentItem? item)
    {
        if (item == null) return;
        CustomDllSelected = ((StrongBox<bool>)item.Tag!).Value;
    }
}
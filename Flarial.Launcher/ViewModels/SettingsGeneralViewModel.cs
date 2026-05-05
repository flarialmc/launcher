using System.Collections.ObjectModel;
using System.Linq;
using Flarial.Launcher.Controls.SegmentedBar;
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

    public SettingsGeneralViewModel()
    {
        
        BuildTypes =
        [
            new SegmentItem { Title = "Release", Tag = 0, Tooltip = "Latest stable release" },
            new SegmentItem { Title = "Beta", Tag = 1, Tooltip = "Less stable release, expect crashes and bugs"},
            new SegmentItem { Title = "Nightly", IsEnabled = false, Tag = 2 },
            new SegmentItem { Title = "Custom", Tag = 3, Tooltip = "Input your own custom DLL to use a client other than Flarial"}
        ];

        SelectedBuild = BuildTypes.FirstOrDefault();
    }

    private void OnBuildChanged(SegmentItem? item)
    {
        if (item == null) return;
        
        CustomDllSelected = item.Title == "Custom";
    }
}
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Flarial.Launcher.Controls.SegmentedBar;
using Flarial.Launcher.Management;

namespace Flarial.Launcher.ViewModels;

public class SettingsGeneralViewModel : ViewModelBase
{
    readonly AppSettings _settings;
    readonly object _saveLock = new();
    Timer? _customDllPathSaveTimer;

    public bool CustomDllSelected
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool AutomaticUpdates
    {
        get => _settings.AutomaticUpdates;
        set
        {
            if (_settings.AutomaticUpdates == value) return;

            _settings.AutomaticUpdates = value;
            _settings.Set();
            this.RaisePropertyChanged();
        }
    }

    public bool HardwareAcceleration
    {
        get => _settings.HardwareAcceleration;
        set
        {
            if (_settings.HardwareAcceleration == value) return;

            _settings.HardwareAcceleration = value;
            _settings.Set();
            this.RaisePropertyChanged();
        }
    }

    public string CustomDllPath
    {
        get => _settings.CustomDllPath;
        set
        {
            if (_settings.CustomDllPath == value) return;

            _settings.CustomDllPath = value;
            SaveSettingsDebounced();
            this.RaisePropertyChanged();
        }
    }

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

    public SettingsGeneralViewModel(AppSettings settings)
    {
        _settings = settings;
        BuildTypes =
        [
            new SegmentItem { Title = "Release", Tag = 0, Tooltip = "Latest stable release" },
            new SegmentItem { Title = "Beta", IsEnabled = false, Tag = 1, Tooltip = "Less stable release, expect crashes and bugs" },
            new SegmentItem { Title = "Nightly", IsEnabled = false, Tag = 2 },
            new SegmentItem { Title = "Custom", Tag = 3, Tooltip = "Input your own custom DLL to use a client other than Flarial" }
        ];

        SelectedBuild = BuildTypes.FirstOrDefault(item => item.Title == (_settings.UseCustomDll ? "Custom" : "Release"));
    }

    void OnBuildChanged(SegmentItem? item)
    {
        if (item is null) return;

        _settings.UseCustomDll = item.Title == "Custom";
        _settings.Set();
        CustomDllSelected = _settings.UseCustomDll;
    }

    void SaveSettingsDebounced()
    {
        _customDllPathSaveTimer ??= new Timer(_ =>
        {
            lock (_saveLock)
                _settings.Set();
        });

        _customDllPathSaveTimer.Change(500, Timeout.Infinite);
    }
}

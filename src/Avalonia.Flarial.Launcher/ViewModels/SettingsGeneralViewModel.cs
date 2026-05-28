using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using Flarial.Launcher.Controls.SegmentedBar;
using Flarial.Launcher.Management;
using Microsoft.Win32;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.ViewModels;

public partial class SettingsGeneralViewModel : ViewModelBase
{
    [Reactive]
    string? _customDllPath;

    [Reactive]
    bool _customDllSelected;

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

    readonly OpenFileDialog _openFileDialog = new()
    {
        ValidateNames = true,
        DereferenceLinks = true,
        CheckFileExists = true,
        CheckPathExists = true,
        ReadOnlyChecked = true,
        RestoreDirectory = false,
        Filter = "Dynamic-Link Libraries (*.dll)|*.dll"
    };

    public ReactiveCommand<Unit, Unit> Open { get; }

    void OnOpen()
    {
        if (_openFileDialog.ShowDialog() ?? false)
        {
            CustomDllPath = _openFileDialog.FileName;
            _appSettings.CustomDllPath = _openFileDialog.FileName;
        }
    }

    readonly AppSettings _appSettings;

    public SettingsGeneralViewModel(AppSettings appSettings)
    {
        _appSettings = appSettings;

        BuildTypes = [new() { Title = "Release", Tag = false }, new() { Title = "Custom", Tag = true }];

        SelectedBuild = _appSettings.UseCustomDll switch
        {
            false => BuildTypes[0],
            true => BuildTypes[1]
        };

        CustomDllPath = _appSettings.CustomDllPath;
        CustomDllSelected = _appSettings.UseCustomDll;
        AutomaticUpdates = _appSettings.AutomaticUpdates;

        Open = ReactiveCommand.Create(OnOpen);
    }

    private void OnBuildChanged(SegmentItem? item)
    {
        if (item == null) return;
        var value = (bool)item.Tag!;

        CustomDllSelected = value;
        _appSettings.UseCustomDll = value;
    }
}
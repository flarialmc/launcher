using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Flarial.Launcher.Controls.SegmentedBar;
using Flarial.Launcher.Management;
using Flarial.Runtime.Unmanaged;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.ViewModels;

public partial class SettingsGeneralViewModel : ViewModelBase
{
    [Reactive]
    private string? _customDllPath;

    [Reactive]
    private bool _customDllSelected;

    public AvaloniaList<SegmentItem> BuildTypes { get; }

    public SegmentItem? SelectedBuild
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            OnBuildChanged(field);
        }
    }

    public bool PerformanceMode
    {
        get;
        set
        {
            _settings.PerformanceMode = value;
            this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    public bool AutomaticUpdates
    {
        get;
        set
        {
            _settings.AutomaticUpdates = value;
            this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    static readonly FilePickerOpenOptions s_options = new()
    {
        FileTypeFilter = [new("Dynamic Link Libraries") { Patterns = ["*.dll"] }]
    };

    public ReactiveCommand<Unit, Unit> OpenClientFolder { get; }

    public ReactiveCommand<Unit, Unit> OpenLauncherFolder { get; }

    public ReactiveCommand<Unit, Unit> Open { get; }

    async Task OnOpenAsync()
    {
        var application = Application.Current!;
        var lifetime = (IClassicDesktopStyleApplicationLifetime)application.ApplicationLifetime!;
        var files = await lifetime.MainWindow!.StorageProvider.OpenFilePickerAsync(s_options);

        if (files.Any())
        {
            var path = files[0].TryGetLocalPath()!;
            CustomDllPath = _settings.CustomDllPath = path;
        }
    }

    void OnOpenLauncherFolder() => NativePlatform.Open(".");

    void OnOpenClientFolder() => NativePlatform.Open(Directory.CreateDirectory(@"..\Client").FullName);

    readonly AppSettings _settings = ((App)Application.Current!).Settings;

    public SettingsGeneralViewModel()
    {
        BuildTypes = [new() { Title = "Flarial Client", Tag = false }, new() { Title = "Custom DLL", Tag = true }];

        SelectedBuild = _settings.UseCustomDll switch
        {
            false => BuildTypes[0],
            true => BuildTypes[1]
        };

        CustomDllPath = _settings.CustomDllPath;
        CustomDllSelected = _settings.UseCustomDll;
        PerformanceMode = _settings.PerformanceMode;
        AutomaticUpdates = _settings.AutomaticUpdates;

        Open = ReactiveCommand.CreateFromTask(OnOpenAsync);
        OpenClientFolder = ReactiveCommand.Create(OnOpenClientFolder);
        OpenLauncherFolder = ReactiveCommand.Create(OnOpenLauncherFolder);
    }

    private void OnBuildChanged(SegmentItem? item)
    {
        if (item == null) return;
        var value = (bool)item.Tag!;

        CustomDllSelected = value;
        _settings.UseCustomDll = value;
    }
}
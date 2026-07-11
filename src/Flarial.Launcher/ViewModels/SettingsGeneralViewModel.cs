using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Flarial.Launcher.Controls.SegmentedBar;
using Flarial.Launcher.Management;
using Flarial.Launcher.Models;
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

    public ReactiveCommand<Unit, Unit> LoginCommand { get; }

    
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

    void OnOpenLauncherFolder() => NativeMethods.ShellExecute(".");

    void OnOpenClientFolder() => NativeMethods.ShellExecute(Directory.CreateDirectory(@"..\Client").FullName);

    readonly AppSettings _settings = ((App)Application.Current!).Settings;

    public UserState UserState { get; }

    public SettingsGeneralViewModel(UserState userState)
    {
        UserState = userState;
        
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
        LoginCommand = ReactiveCommand.CreateFromTask(Login);
    }

    private Task Login()
    {
        if (UserState.Username == "Guest")
        {
            UserState.Username = "bari2d";
            UserState.PfpSource = new Uri("https://images-ext-1.discordapp.net/external/4pF2LzTIVXyoEkFxIUqjfp1Z9msFn0nIBONUMCIpF6I/%3Fsize%3D4096/https/cdn.discordapp.com/avatars/546211976674803712/cfedecac78770d26c1fff64bb2df31e9.png", UriKind.Absolute);
            UserState.Role = new Role { Name = "Flarial+", BackgroundBrush = SolidColorBrush.Parse("#40FF2438"), BorderBrush = SolidColorBrush.Parse("#FFFF2438")};
        }
        else
        {
            UserState.Username = "Guest";
            UserState.PfpSource = new Uri("avares://Flarial.Launcher/Assets/person_96dp_FF2438.png", UriKind.Absolute);
            UserState.Role = new Role();
        }
        
        return Task.CompletedTask;
    }
    
    private void OnBuildChanged(SegmentItem? item)
    {
        if (item == null) return;
        var value = (bool)item.Tag!;

        CustomDllSelected = value;
        _settings.UseCustomDll = value;
    }
}
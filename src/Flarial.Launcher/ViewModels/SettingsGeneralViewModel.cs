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
using Flarial.Runtime.Discord;
using Flarial.Runtime.Unmanaged;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.ViewModels;

public partial class SettingsGeneralViewModel : ViewModelBase
{
    [Reactive] string? _customDllPath = null;
    [Reactive] bool _customDllSelected = false;
    [Reactive] bool _discordLoginActive = false;

    public DiscordAccount Account => _model._account;
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

    public ReactiveCommand<Unit, Unit> Open { get; }
    public ReactiveCommand<Unit, Unit> Login { get; }
    public ReactiveCommand<Unit, Unit> OpenClientFolder { get; }
    public ReactiveCommand<Unit, Unit> OpenLauncherFolder { get; }

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

    readonly AppSettings _settings;
    readonly MainWindowViewModel _model;

    public SettingsGeneralViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _model = mainWindowViewModel;
        _settings = ((App)Application.Current!).Settings;

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
        Login = ReactiveCommand.CreateFromTask(OnLoginAsync);
        OpenClientFolder = ReactiveCommand.Create(OnOpenClientFolder);
        OpenLauncherFolder = ReactiveCommand.Create(OnOpenLauncherFolder);
    }

    async Task OnLoginAsync()
    {
        DiscordLoginActive = true; try
        {
            var code = await DiscordAccountManager.GetAuthorizationCodeAsync();
            throw new(code);
        }
        finally { DiscordLoginActive = false; }
    }

    private void OnBuildChanged(SegmentItem? item)
    {
        if (item == null) return;
        var value = (bool)item.Tag!;

        CustomDllSelected = value;
        _settings.UseCustomDll = value;
    }
}
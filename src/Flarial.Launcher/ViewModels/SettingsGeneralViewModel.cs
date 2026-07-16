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
using Splat;

namespace Flarial.Launcher.ViewModels;

public partial class SettingsGeneralViewModel : ViewModelBase
{
    [Reactive] string? _customDllPath = null;
    [Reactive] bool _customDllSelected = false;
    [Reactive] bool _discordLoginActive = true;
    [Reactive] bool _discordLoginAvailable = false;
    [Reactive] bool _discordAccountAvailable = false;

    public AvaloniaList<SegmentItem> BuildTypes { get; }
    public DiscordAccountModel DiscordAccount => _model._discordAccount;

    readonly SegmentItem _customItem = new() { Title = "Custom", Tag = BuildType.Custom };
    readonly SegmentItem _releaseItem = new() { Title = "Release", Tag = BuildType.Release };
    readonly SegmentItem _betaItem = new() { Title = "Beta", Tag = BuildType.Beta, IsEnabled = false };

    internal bool HasBetaAccess
    {
        get;
        set
        {
            if (SelectedBuild == _betaItem && !value)
                SelectedBuild = _releaseItem;
            _betaItem.IsEnabled = field = value;
        }
    }

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
    public ReactiveCommand<Unit, Unit> Logout { get; }
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

        BuildTypes = [_releaseItem, _betaItem, _customItem];

        switch (_settings.BuildType)
        {
            case BuildType.Beta:
                SelectedBuild = _betaItem;
                break;

            case BuildType.Release:
                SelectedBuild = _releaseItem;
                break;

            case BuildType.Custom:
                SelectedBuild = _customItem;
                CustomDllSelected = true;
                break;
        }

        CustomDllPath = _settings.CustomDllPath;
        PerformanceMode = _settings.PerformanceMode;
        AutomaticUpdates = _settings.AutomaticUpdates;

        Logout = ReactiveCommand.Create(OnLogout);
        Open = ReactiveCommand.CreateFromTask(OnOpenAsync);
        Login = ReactiveCommand.CreateFromTask(OnLoginAsync);
        OpenClientFolder = ReactiveCommand.Create(OnOpenClientFolder);
        OpenLauncherFolder = ReactiveCommand.Create(OnOpenLauncherFolder);
    }

    async Task OnLoginAsync()
    {
        DiscordLoginAvailable = false;

        if (!await OAuthManager.AuthenticateAsync())
        {
            OnLogout();
            return;
        }

        await LoginAsync();
    }

    internal async Task LoginAsync()
    {
        DiscordLoginAvailable = false;

        if (await DiscordAccountManager.LoginAsync() is not { } account)
        {
            OnLogout();
            return;
        }

        HasBetaAccess = account.HasBetaAccess;
        _model.HomeViewModel.ShowPromotions = !account.HasFlarialPlus;

        DiscordAccountAvailable = true;
        DiscordAccount.Login(account);
    }

    void OnLogout()
    {
        HasBetaAccess = false;
        _model.HomeViewModel.ShowPromotions = true;

        DiscordAccountManager.Logout();
        DiscordAccount.Logout();

        DiscordLoginAvailable = true;
        DiscordAccountAvailable = false;
    }

    private void OnBuildChanged(SegmentItem? item)
    {
        if (item == null) return;
        var buildType = (BuildType)item.Tag!;

        _settings.BuildType = buildType;
        CustomDllSelected = buildType is BuildType.Custom;
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using Flarial.Launcher.Dialogs;
using Flarial.Launcher.Dialogs.Metadata;
using Flarial.Launcher.Management;
using Flarial.Launcher.Models;
using Flarial.Runtime.Core;
using Flarial.Runtime.Discord;
using Flarial.Runtime.Game;
using Flarial.Runtime.Versions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    readonly SemaphoreSlim _semaphore = new(1, 1);

    public MessageBoxViewModel? CurrentDialog
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public HomeViewModel HomeViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }
    public NotificationAreaViewModel NotificationArea { get; }
    public VersionRegistry VersionRegistry { get; private set; }

    internal readonly DiscordAccountModel _discordAccount;
    readonly AppSettings _settings = ((App)Application.Current!).Settings;

    public MainWindowViewModel()
    {
        HomeViewModel = new HomeViewModel(this);
        SettingsViewModel = new SettingsViewModel(this);
        NotificationArea = new NotificationAreaViewModel();

        VersionRegistry = null!;
        _discordAccount = new();
    }

    public async Task<string> ShowMessageBoxAsync(string title, string message, IEnumerable<string> buttons)
    {
        await _semaphore.WaitAsync(); try
        {
            try
            {
                CurrentDialog = new(title, message, buttons);
                return await CurrentDialog.Result;
            }
            finally { CurrentDialog = null; }
        }
        finally { _semaphore.Release(); }
    }

    void OnDownload(int value) => HomeViewModel.LauncherStatus = $"Updating... {value}%";

    async Task LoginWithDiscordAsync()
    {
        await SettingsViewModel.SettingsGeneralViewModel.LoginAsync();
        SettingsViewModel.SettingsGeneralViewModel.DiscordLoginActive = false;
    }

    public async void OnLoaded()
    {

        if (await FlarialLauncher.CheckForUpdatesAsync() && (_settings.AutomaticUpdates || await LauncherUpdateAvailableDialog._.ShowAsync()))
        {
            await FlarialLauncher.DownloadAsync(OnDownload);
            return;
        }

        var loginWithDiscordTask = LoginWithDiscordAsync();
        VersionRegistry = await VersionRegistry.GetAsync();

        _ = Task.Run(() =>
        {
            foreach (var version in VersionRegistry) Dispatcher.UIThread.Post(() =>
            {
                SettingsViewModel.SettingsVersionsViewModel.Versions.Add(new(this, version));
            }, DispatcherPriority.Background);
        });

        HomeViewModel.OnPackageStatusChanged();
        Minecraft.PackageStatusChanged += HomeViewModel.OnPackageStatusChanged;

        await loginWithDiscordTask;

        HomeViewModel.LauncherStatus = "Ready!";
        HomeViewModel.IsLaunching = false;
    }
}
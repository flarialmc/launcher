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
using Splat;

namespace Flarial.Launcher.ViewModels;

public class MainWindowViewModel : ViewModelBase
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

    internal readonly DiscordAccountModel _discordAccount = new();
    readonly AppSettings _settings = ((App)Application.Current!).Settings;

    public MainWindowViewModel()
    {
        VersionRegistry = null!;
        HomeViewModel = new HomeViewModel(this);
        SettingsViewModel = new SettingsViewModel(this);
        NotificationArea = new NotificationAreaViewModel();
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

    public async void OnLoaded()
    {
        var settingsGeneralViewModel = SettingsViewModel.SettingsGeneralViewModel;

        if (await FlarialLauncher.CheckForUpdatesAsync() && (_settings.AutomaticUpdates || await LauncherUpdateAvailableDialog._.ShowAsync()))
        {
            await FlarialLauncher.DownloadAsync(OnDownload);
            return;
        }

        var versionRegistryTask = VersionRegistry.GetAsync();
        var loginTask = settingsGeneralViewModel.LoginAsync();

        await Task.WhenAll(versionRegistryTask, loginTask);
        settingsGeneralViewModel.DiscordLoginActive = false;

        VersionRegistry = await versionRegistryTask; _ = Task.Run(() =>
        {
            foreach (var version in VersionRegistry) Dispatcher.UIThread.Post(() =>
            {
                SettingsViewModel.SettingsVersionsViewModel.Versions.Add(new(this, version));
            }, DispatcherPriority.Background);
        });

        HomeViewModel.OnPackageStatusChanged();
        Minecraft.PackageStatusChanged += HomeViewModel.OnPackageStatusChanged;

        HomeViewModel.LauncherStatus = "Ready!";
        HomeViewModel.IsLaunching = false;
    }
}
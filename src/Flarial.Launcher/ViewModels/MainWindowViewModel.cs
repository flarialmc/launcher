using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Flarial.Launcher.Dialogs;
using Flarial.Launcher.Dialogs.Metadata;
using Flarial.Launcher.Management;
using Flarial.Runtime.Core;
using Flarial.Runtime.Game;
using Flarial.Runtime.Versions;
using ReactiveUI;

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

    public

    readonly AppSettings _appSettings;

    public MainWindowViewModel(AppSettings appSettings)
    {
        _appSettings = appSettings;

        HomeViewModel = new(this, appSettings);
        SettingsViewModel = new(appSettings);
        NotificationArea = new();
        VersionRegistry = null!;
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

    void OnLauncherDownload(int value) => Dispatcher.UIThread.Invoke(() =>
    {
        HomeViewModel.LauncherStatus = $"Updating... {value}%";
    });

    public async void OnLoaded()
    {
        if (!await FlarialLauncher.VerifyConnectionAsync())
        {
            await ConnectionFailureDialog._.ShowAsync();
            Environment.Exit(0);
            return;
        }

        if (await FlarialLauncher.CheckForUpdatesAsync() && (_appSettings.AutomaticUpdates || await LauncherUpdateAvailableDialog._.ShowAsync()))
        {
            await FlarialLauncher.DownloadAsync(OnLauncherDownload);
            return;
        }

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

        HomeViewModel.LauncherStatus = "Ready!";
        HomeViewModel.IsInitialized = true;
    }
}
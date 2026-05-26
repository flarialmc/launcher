using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
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

    public MainWindowViewModel(AppSettings appSettings)
    {
        HomeViewModel = new(this);
        SettingsViewModel = new(appSettings);
        NotificationArea = new();
        VersionRegistry = null!;
    }

    public async Task<string> ShowMessageBoxAsync(string title, string message, string[] buttons)
    {
        await _semaphore.WaitAsync();
        try
        {
            MessageBoxViewModel dialog = new(title, message, buttons);
            CurrentDialog = dialog;

            var result = await dialog.Result;

            CurrentDialog = null;
            return result;
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
            await ConnectionFailureDialog.ShowAsync();
            Environment.Exit(0);
            return;
        }

        if (await BetaBuildDialog.ShowAsync() && await FlarialLauncher.CheckForUpdatesAsync() && await LauncherUpdateAvailableDialog.ShowAsync())
        {
            await FlarialLauncher.DownloadAsync(OnLauncherDownload);
            return;
        }

        VersionRegistry = await VersionRegistry.CreateAsync();

        foreach (var version in VersionRegistry) Dispatcher.UIThread.Post(() =>
        {
            var versions = SettingsViewModel.SettingsVersionsViewModel.Versions;
            versions.Add(new(this, version));
        }, DispatcherPriority.Background);

        HomeViewModel.OnPackageStatusChanged();
        Minecraft.PackageStatusChanged += HomeViewModel.OnPackageStatusChanged;

        HomeViewModel.LauncherStatus = "Ready!";
        HomeViewModel.IsInitialized = true;
    }
}
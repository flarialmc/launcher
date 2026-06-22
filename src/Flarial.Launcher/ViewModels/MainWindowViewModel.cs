using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
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

    readonly AppSettings _settings = ((App)Application.Current!).Settings;

    public MainWindowViewModel()
    {
        HomeViewModel = new(this);
        SettingsViewModel = new();
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

    void OnDownload(int value) => HomeViewModel.LauncherStatus = $"Updating... {value}%";

    public async void OnLoaded()
    {
        if (!await FlarialLauncher.CheckConnectionAsync())
        {
            await FlarialServicesUnreachableDialog._.ShowAsync();
            Environment.Exit(1);
            return;
        }

        if (await FlarialLauncher.CheckForUpdatesAsync() && (_settings.AutomaticUpdates || await LauncherUpdateAvailableDialog._.ShowAsync()))
        {
            await FlarialLauncher.DownloadAsync(OnDownload);
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
        HomeViewModel.IsLaunching = false;
    }
}
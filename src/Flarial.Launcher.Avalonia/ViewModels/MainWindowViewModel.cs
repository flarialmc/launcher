using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Flarial.Launcher.Dialogs.Metadata;
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

    public MainWindowViewModel()
    {
        HomeViewModel = new(this);
        SettingsViewModel = new();
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

    public async void OnLoaded()
    {
        if (!await FlarialLauncher.VerifyConnectionAsync())
        {
            await ConnectionFailureDialog.ShowAsync();
            Environment.Exit(0);
            return;
        }

        VersionRegistry = await VersionRegistry.CreateAsync();

        var task = Task.Run(() =>
        {
            ObservableCollection<VersionItemViewModel> versions = [];
            foreach (var version in VersionRegistry) versions.Add(new(this, version));
            Dispatcher.UIThread.Invoke(() => SettingsViewModel.SettingsVersionsViewModel.Versions = versions);
        });

        HomeViewModel.OnPackageStatusChanged();
        Minecraft.PackageStatusChanged += HomeViewModel.OnPackageStatusChanged;

        HomeViewModel.LauncherStatus = "Ready!";
        HomeViewModel.IsInitialized = true;

        await task;
        SettingsViewModel.SettingsVersionsViewModel.IsInstalling = false;
    }
}
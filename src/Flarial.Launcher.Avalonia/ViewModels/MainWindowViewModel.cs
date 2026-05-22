using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Flarial.Launcher.Dialogs;
using Flarial.Launcher.Dialogs.Metadata;
using Flarial.Launcher.Services;
using Flarial.Runtime.Versions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

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

    public MainWindowViewModel()
    {
        HomeViewModel = new();
        SettingsViewModel = new();
        NotificationArea = new();
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
        var registry = await VersionRegistry.CreateAsync();
        _ = registry;

        HomeViewModel.LauncherStatus = "Ready!";
        HomeViewModel.IsInitialized = true;
    }
}
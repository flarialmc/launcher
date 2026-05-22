using System;
using System.Threading;
using System.Threading.Tasks;
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
        VersionRegistry = await VersionRegistry.CreateAsync();

        HomeViewModel.OnPackageStatusChanged();
        Minecraft.PackageStatusChanged += HomeViewModel.OnPackageStatusChanged;

        HomeViewModel.LauncherStatus = "Ready!";
        HomeViewModel.IsInitialized = true;
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flarial.Launcher.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDialogService, INotificationService
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
        HomeViewModel = new(this, this);
        SettingsViewModel = new();
        NotificationArea = new();
    }

    public async Task<string> ShowMessageBoxAsync(string title, string message, IReadOnlyList<string> buttons)
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

    void INotificationService.Show(string message) => NotificationArea.Add(message);
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flarial.Launcher.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDialogService, INotificationService
{
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

    [Obsolete("", true)]
    public Task InitializeSettingsAsync()
    {
        return Task.CompletedTask;
    }

    public async Task<string> ShowMessageBoxAsync(string title, string message, IEnumerable<string> buttons)
    {
        var dialog = new MessageBoxViewModel(title, message, buttons);
        CurrentDialog = dialog;

        var result = await dialog.Result;

        CurrentDialog = null;
        return result;
    }

    void INotificationService.Show(string message) => NotificationArea.Add(message);
}
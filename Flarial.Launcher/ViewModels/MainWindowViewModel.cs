using System.Collections.Generic;
using System.Threading.Tasks;
using Flarial.Launcher.Services;
using ReactiveUI;

namespace Flarial.Launcher.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDialogService, INotificationService
{
    public MessageBoxViewModel? CurrentDialog
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public HomeViewModel HomeViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; } = new();
    public NotificationAreaViewModel NotificationArea { get; } = new();
    
    public MainWindowViewModel()
    {
        HomeViewModel = new HomeViewModel(this, this);
    }

    
    public async Task<string> ShowMessageBoxAsync(
        string title,
        string message,
        IEnumerable<string> buttons)
    {
        var dialog = new MessageBoxViewModel(title, message, buttons);
        CurrentDialog = dialog;

        var result = await dialog.Result;

        CurrentDialog = null;
        return result;
    }
    
    void INotificationService.Show(string message) => NotificationArea.Add(message);
}
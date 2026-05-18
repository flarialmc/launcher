using System;
using System.Windows.Input;

namespace Flarial.Launcher.ViewModels;

public class NotificationViewModel : ViewModelBase
{
    public string Message { get; }
    
    public event Action<NotificationViewModel>? CloseRequested;

    public ICommand CloseCommand { get; }

    private readonly Action _onDismissed;

    public NotificationViewModel(string message, Action onDismissed)
    {
        Message = message;
        _onDismissed = onDismissed;
        CloseCommand = new RelayCommand(() => CloseRequested?.Invoke(this));
    }
    
    public void CompleteDismiss() => _onDismissed();
}

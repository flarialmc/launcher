using System.Collections.ObjectModel;

namespace Flarial.Launcher.ViewModels;

public class NotificationAreaViewModel : ViewModelBase
{
    public ObservableCollection<NotificationViewModel> Notifications { get; } = [];

    public void Add(string message)
    {
        NotificationViewModel? vm = null;
        vm = new NotificationViewModel(message, onDismissed: () => Remove(vm!));
        Notifications.Add(vm);
    }
    
    public void Remove(NotificationViewModel vm) =>
        Notifications.Remove(vm);
}
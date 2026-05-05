using System;
using System.Reactive;
using System.Reactive.Subjects;
using ReactiveUI;

namespace Flarial.Launcher.ViewModels;

public class NotificationViewModel : ReactiveObject
{
    public string Message { get; }
    
    public Subject<Unit> CloseRequested { get; } = new();

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    private readonly Action _onDismissed;

    public NotificationViewModel(string message, Action onDismissed)
    {
        Message = message;
        _onDismissed = onDismissed;
        CloseCommand = ReactiveCommand.Create(() => CloseRequested.OnNext(Unit.Default));
    }
    
    public void CompleteDismiss() => _onDismissed();
}
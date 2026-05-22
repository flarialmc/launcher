using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Security.Permissions;
using System.Threading.Tasks;
using ReactiveUI;

namespace Flarial.Launcher.ViewModels;

public class MessageBoxViewModel : ReactiveObject
{
    private readonly TaskCompletionSource<string> _tcs = new();
    private string? _pendingResult;

    public string Title { get; }
    public string Message { get; }
    public string[] Buttons { get; }
    public Subject<Unit> CloseRequested { get; } = new();

    public ReactiveCommand<string, Unit> SelectButtonCommand { get; }

    public Task<string> Result => _tcs.Task;

    public MessageBoxViewModel(string title, string message, string[] buttons)
    {
        Title = title;
        Message = message;
        Buttons = buttons;

        SelectButtonCommand = ReactiveCommand.Create<string>(button =>
        {
            _pendingResult = button;
            CloseRequested.OnNext(Unit.Default);
        });
    }

    public void CompleteClose() => _tcs.TrySetResult(_pendingResult ?? string.Empty);
}
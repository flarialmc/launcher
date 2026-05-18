using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Flarial.Launcher.ViewModels;

public class MessageBoxViewModel : ViewModelBase
{
    private readonly TaskCompletionSource<string> _tcs = new();
    private string? _pendingResult;

    public string Title { get; }
    public string Message { get; }
    public IReadOnlyList<string> Buttons { get; }
    public event Action<MessageBoxViewModel>? CloseRequested;

    public ICommand SelectButtonCommand { get; }

    public Task<string> Result => _tcs.Task;

    public MessageBoxViewModel(string title, string message, IEnumerable<string> buttons)
    {
        Title = title;
        Message = message;
        Buttons = buttons.ToList();

        SelectButtonCommand = new RelayCommand<string>(button =>
        {
            _pendingResult = button;
            CloseRequested?.Invoke(this);
        });
    }
    
    public void CompleteClose() => _tcs.TrySetResult(_pendingResult ?? string.Empty);
}

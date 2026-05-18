using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Flarial.Launcher.Types;

namespace Flarial.Launcher.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    readonly RelayCommand<PageTransitions> _navigateCommand;
    bool _isAnimating;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsAnimating
    {
        get => _isAnimating;
        set
        {
            RaiseAndSetIfChanged(ref _isAnimating, value);
            _navigateCommand.RaiseCanExecuteChanged();
        }
    }

    public ICommand NavigateCommand => _navigateCommand;

    protected ViewModelBase()
        => _navigateCommand = new(AppMessageBus.Send, _ => !IsAnimating);

    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool RaiseAndSetIfChanged<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        RaisePropertyChanged(propertyName);
        return true;
    }
}

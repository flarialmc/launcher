using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Flarial.Launcher.ViewModels;
using Flarial.Launcher.Views;

namespace Flarial.Launcher.Dialogs;

public abstract class MessageDialog<T> : MessageDialog where T : MessageDialog<T>, new()
{
    static readonly MessageDialog s_instance = new T();

    internal static async Task<bool> ShowAsync() => await s_instance.OnShowAsync();
}

public abstract class MessageDialog
{
    protected abstract string Title { get; }
    protected abstract string Message { get; }
    protected abstract string[] Buttons { get; }

    readonly Dictionary<string, int> _buttons = [];

    protected MessageDialog()
    {
        for (var index = 0; index < Buttons.Length; index++)
            _buttons.Add(Buttons[index], index);
    }

    internal virtual async Task<bool> OnShowAsync()
    {
        var application = Application.Current!;
        var lifetime = (IClassicDesktopStyleApplicationLifetime)application.ApplicationLifetime!;

        var view = (MainWindow)lifetime.MainWindow!;
        var model = (MainWindowViewModel)view.DataContext!;

        return _buttons[await model.ShowMessageBoxAsync(Title, Message, Buttons)] <= 0;
    }
}
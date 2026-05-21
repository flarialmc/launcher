using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Flarial.Launcher.ViewModels;
using Flarial.Launcher.Views;

namespace Flarial.Launcher.Dialogs;

public abstract class MessageDialog
{
    protected abstract string Title { get; }
    protected abstract string Message { get; }
    protected abstract IReadOnlyList<string> Buttons { get; }

    readonly Dictionary<string, int> _buttons = [];
    static readonly Dictionary<Type, MessageDialog> s_dialogs = [];

    protected MessageDialog()
    {
        for (var index = 0; index < Buttons.Count; index++)
            _buttons.Add(Buttons[index], index);
    }

    static MessageDialog Get<T>() where T : MessageDialog, new()
    {
        if (!s_dialogs.TryGetValue(typeof(T), out var dialog))
        {
            dialog = new T();
            s_dialogs.Add(typeof(T), dialog);
        }
        return dialog;
    }

    protected virtual async Task<int> ShowAsync()
    {
        var application = Application.Current!;
        var lifetime = (IClassicDesktopStyleApplicationLifetime)application.ApplicationLifetime!;

        var view = (MainWindow)lifetime.MainWindow!;
        var model = (MainWindowViewModel)view.DataContext!;

        return _buttons[await model.ShowMessageBoxAsync(Title, Message, Buttons)];
    }

    public static async Task<int> ShowAsync<T>() where T : MessageDialog, new() => await Get<T>().ShowAsync();
}
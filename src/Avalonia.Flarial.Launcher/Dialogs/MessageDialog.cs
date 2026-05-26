using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Flarial.Launcher.ViewModels;
using Flarial.Launcher.Views;

namespace Flarial.Launcher.Dialogs;

public abstract class MessageDialog<T> : MessageDialog where T : MessageDialog<T>, new()
{
    static readonly ConcurrentDictionary<Type, MessageDialog<T>> s_dialogs = [];

    static MessageDialog<T> Get() => s_dialogs.GetOrAdd(typeof(T), static type => new T());

    internal static async Task<bool> ShowAsync() => await Get().OnShowAsync();
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
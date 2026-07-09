using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Flarial.Launcher.ViewModels;
using Flarial.Launcher.Views;

namespace Flarial.Launcher.Dialogs;

abstract class MessageDialog<T> : MessageDialog where T : MessageDialog<T>, new()
{
    private protected MessageDialog()
    {
        if (_ is null) return;
        throw new InvalidOperationException();
    }

    internal static readonly T _ = new();
}

abstract class MessageDialog
{
    protected abstract string Title { get; }
    protected abstract string Message { get; }
    protected abstract string Primary { get; }
    protected virtual string? Secondary { get; }

    readonly Dictionary<string, int> _buttons = [];

    protected MessageDialog()
    {
        if (Primary is { }) _buttons[Primary] = 0;
        if (Secondary is { }) _buttons[Secondary] = 1;
    }

    protected virtual async Task OnShowAsync(bool value) { }

    internal async Task<bool> ShowAsync()
    {
        var application = Application.Current!;
        var lifetime = (IClassicDesktopStyleApplicationLifetime)application.ApplicationLifetime!;

        var view = (MainWindow)lifetime.MainWindow!;
        var model = (MainWindowViewModel)view.DataContext!;
        var key = await model.ShowMessageBoxAsync(Title, Message, _buttons.Keys);

        var value = _buttons[key] <= 0;
        await OnShowAsync(value);

        return value;
    }
}
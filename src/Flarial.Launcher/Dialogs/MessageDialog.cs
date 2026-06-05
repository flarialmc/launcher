using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Flarial.Launcher.ViewModels;
using Flarial.Launcher.Views;
using Windows.Security.Cryptography.Core;

namespace Flarial.Launcher.Dialogs;

public abstract class MessageDialog<T> : MessageDialog where T : MessageDialog<T>, new()
{
    internal static readonly T s_this = new();
}

public static class MessageDialogMembers
{
    extension<T>(MessageDialog<T>) where T : MessageDialog<T>, new()
    {
        internal static async Task<bool> ShowAsync() => await MessageDialog<T>.s_this.ShowAsync();
    }
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

    protected virtual async Task OnShowAsync(bool value) { }

    internal async Task<bool> ShowAsync()
    {
        var application = Application.Current!;
        var lifetime = (IClassicDesktopStyleApplicationLifetime)application.ApplicationLifetime!;

        var view = (MainWindow)lifetime.MainWindow!;
        var model = (MainWindowViewModel)view.DataContext!;
        var key = await model.ShowMessageBoxAsync(Title, Message, Buttons);
        
        var value = _buttons[key] <= 0;
        await OnShowAsync(value);
        
        return value;
    }
}
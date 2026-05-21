using System.Reactive;
using System.Reflection;
using Flarial.Launcher.Services;
using Flarial.Launcher.Types;
using ReactiveUI;

namespace Flarial.Launcher.ViewModels;

public class HomeViewModel : ViewModelBase
{
    static readonly string _launcherVersion;

    static HomeViewModel()
    {
        var assembly = Assembly.GetExecutingAssembly();
        _launcherVersion = $"{assembly.GetName().Version}";
    }

    public string GameVersion { get; } = "0.0.0";
    public string LauncherVersion { get; } = _launcherVersion;

    public HomeViewModel(IDialogService dialogService, INotificationService notificationService)
    {
        //var dialogs = dialogService;
        var notifications = notificationService;
        Launch = ReactiveCommand.CreateFromTask(async () =>
            {
                /*var result = await dialogs.ShowMessageBoxAsync(
                    "Test Message Box", 
                    "hello world type shit", 
                    ["button 1", "button 2", "button 3"]);*/
                notifications.Show($"Unable to inject! Flarial is incompatible with your current MC:BE version.");
            });
    }

    public ReactiveCommand<Unit, Unit> Launch { get; }

    public ReactiveCommand<Unit, Unit> MinimizeWindow { get; } =
        ReactiveCommand.Create(() => MessageBus.Current.SendMessage(WindowStateArgs.Minimize));

    public ReactiveCommand<Unit, Unit> CloseWindow { get; } =
        ReactiveCommand.Create(() => MessageBus.Current.SendMessage(WindowStateArgs.Close));
}
using System.Reactive;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Threading;
using Flarial.Launcher.Dialogs;
using Flarial.Launcher.Dialogs.Metadata;
using Flarial.Launcher.Services;
using Flarial.Launcher.Types;
using Flarial.Runtime.Analytics;
using Flarial.Runtime.Core;
using Flarial.Runtime.Game;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    [Reactive] bool _isLaunched = true;
    [Reactive] bool _isInitialized = false;

    [Reactive] string _launcherStatus = "Preparing...";

    [Reactive] string _gameVersion = "0.0.0";
    [Reactive] string _launcherVersion;

    public HomeViewModel()
    {
        var assembly = Assembly.GetExecutingAssembly();
        _launcherVersion = $"{assembly.GetName().Version}";

        Launch = ReactiveCommand.CreateFromTask(OnLaunchClickedAsync);
    }

    async Task OnLaunchClickedAsync()
    {
        IsLaunched = false; try
        {
            if (!Minecraft.IsGamingServicesInstalled)
            {
                await MessageDialog.ShowAsync<GamingServicesMissingDialog>();
                return;
            }

            if (!Minecraft.IsInstalled)
            {
                await MessageDialog.ShowAsync<NotInstalledDialog>();
                return;
            }

            LauncherStatus = "Verifying...";
            if (!await FlarialClient.Current.DownloadAsync(OnDownload))
            {
                await MessageDialog.ShowAsync<ClientUpdateFailureDialog>();
                return;
            }

            LauncherStatus = "Launching...";
            if (!await FlarialClient.Current.TrackedLaunchAsync() ?? false)
            {
                await MessageDialog.ShowAsync<LaunchFailureDialog>();
                return;
            }
        }
        finally
        {
            IsLaunched = true;
            LauncherStatus = "Ready!";
        }
    }

    async void OnDownload(int value) => Dispatcher.UIThread.Invoke(() =>
    {
        LauncherStatus = $"Downloading... {value}%";
    });

    public ReactiveCommand<Unit, Unit> Launch { get; }

    public ReactiveCommand<Unit, Unit> MinimizeWindow { get; } =
        ReactiveCommand.Create(() => MessageBus.Current.SendMessage(WindowStateArgs.Minimize));

    public ReactiveCommand<Unit, Unit> CloseWindow { get; } =
        ReactiveCommand.Create(() => MessageBus.Current.SendMessage(WindowStateArgs.Close));
}
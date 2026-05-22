using System.Reactive;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using Flarial.Launcher.Dialogs;
using Flarial.Launcher.Dialogs.Metadata;
using Flarial.Launcher.Services;
using Flarial.Launcher.Types;
using Flarial.Runtime.Analytics;
using Flarial.Runtime.Core;
using Flarial.Runtime.Game;
using Flarial.Runtime.Versions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    [Reactive] bool _isLaunched = true;
    [Reactive] bool _isInitialized = false;

    [Reactive] string _launcherVersion;
    [Reactive] string _launcherStatus = "Preparing...";

    [Reactive] string _gameVersion = "0.0.0";
    [Reactive] IImmutableSolidColorBrush _gameVersionColor = Brushes.Gray;

    readonly MainWindowViewModel _mainWindowViewModel;

    public HomeViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;

        var assembly = Assembly.GetExecutingAssembly();
        _launcherVersion = $"{assembly.GetName().Version}";

        Launch = ReactiveCommand.CreateFromTask(OnLaunchAsync);
    }

    async Task OnLaunchAsync()
    {
        IsLaunched = false; try
        {
            if (!Minecraft.IsGamingServicesInstalled)
            {
                await GamingServicesMissingDialog.ShowAsync();
                return;
            }

            if (!Minecraft.IsInstalled)
            {
                await NotInstalledDialog.ShowAsync();
                return;
            }

            LauncherStatus = "Verifying...";
            if (!await FlarialClient.Current.DownloadAsync(OnDownload))
            {
                await ClientUpdateFailureDialog.ShowAsync();
                return;
            }

            LauncherStatus = "Launching...";
            if (!await FlarialClient.Current.TrackedLaunchAsync() ?? false)
            {
                await LaunchFailureDialog.ShowAsync();
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

    public void OnPackageStatusChanged()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Invoke(OnPackageStatusChanged);
            return;
        }

        if (!Minecraft.IsInstalled)
        {
            GameVersion = "0.0.0";
            GameVersionColor = Brushes.Gray;
            return;
        }

        GameVersion = VersionRegistry.InstalledVersion;
        GameVersionColor = _mainWindowViewModel.VersionRegistry.IsSupported ? Brushes.DarkGreen : Brushes.DarkRed;
    }

    public ReactiveCommand<Unit, Unit> Launch { get; }

    public ReactiveCommand<Unit, Unit> MinimizeWindow { get; } =
        ReactiveCommand.Create(() => MessageBus.Current.SendMessage(WindowStateArgs.Minimize));

    public ReactiveCommand<Unit, Unit> CloseWindow { get; } =
        ReactiveCommand.Create(() => MessageBus.Current.SendMessage(WindowStateArgs.Close));
}
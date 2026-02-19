using System.Windows;
using Flarial.Launcher.Management;

namespace Flarial.Launcher;

sealed class MainApplication : Application
{
    readonly ApplicationSettings _settings;

    internal MainApplication(ApplicationSettings settings)
    {
        _settings = settings;
    }

    protected override void OnExit(ExitEventArgs args)
    {
        base.OnExit(args);
        _settings.SaveSettings();
    }
}
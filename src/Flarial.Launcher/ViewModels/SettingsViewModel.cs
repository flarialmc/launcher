using Flarial.Launcher.Management;

namespace Flarial.Launcher.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    public SettingsGeneralViewModel SettingsGeneralViewModel { get; }
    public SettingsVersionsViewModel SettingsVersionsViewModel { get; }
    public SettingsConfigsViewModel SettingsConfigsViewModel { get; } = new();

    public SettingsViewModel(AppSettings settings, MainWindowViewModel host, HomeViewModel home)
    {
        SettingsGeneralViewModel = new SettingsGeneralViewModel(settings);
        SettingsVersionsViewModel = new SettingsVersionsViewModel(host, home);
    }
}

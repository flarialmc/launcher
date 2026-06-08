using Flarial.Launcher.Management;

namespace Flarial.Launcher.ViewModels;

public class SettingsViewModel(AppSettings appSettings) : ViewModelBase
{
    public SettingsGeneralViewModel SettingsGeneralViewModel { get; } = new(appSettings);
    public SettingsVersionsViewModel SettingsVersionsViewModel { get; } = new();
    public SettingsConfigsViewModel SettingsConfigsViewModel { get; } = new();
}
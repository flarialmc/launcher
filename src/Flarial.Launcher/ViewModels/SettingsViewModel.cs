using Flarial.Launcher.Management;

namespace Flarial.Launcher.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    public SettingsGeneralViewModel SettingsGeneralViewModel { get; } = new();
    public SettingsVersionsViewModel SettingsVersionsViewModel { get; } = new();
    public SettingsConfigsViewModel SettingsConfigsViewModel { get; } = new();
}
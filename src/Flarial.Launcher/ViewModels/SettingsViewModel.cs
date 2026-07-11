using Flarial.Launcher.Models;

namespace Flarial.Launcher.ViewModels;

public class SettingsViewModel(UserState userState) : ViewModelBase
{
    public SettingsGeneralViewModel SettingsGeneralViewModel { get; } = new(userState);
    public SettingsVersionsViewModel SettingsVersionsViewModel { get; } = new();
    public SettingsConfigsViewModel SettingsConfigsViewModel { get; } = new();
}
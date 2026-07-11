using Flarial.Launcher.Models;

namespace Flarial.Launcher.ViewModels;

public class SettingsViewModel(MainWindowViewModel mainWindowViewModel) : ViewModelBase
{
    public SettingsConfigsViewModel SettingsConfigsViewModel { get; } = new();
    public SettingsVersionsViewModel SettingsVersionsViewModel { get; } = new();
    public SettingsGeneralViewModel SettingsGeneralViewModel { get; } = new(mainWindowViewModel);
}
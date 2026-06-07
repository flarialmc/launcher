using System.Reactive;
using Flarial.Launcher.Management;
using Flarial.Launcher.Types;
using ReactiveUI;

namespace Flarial.Launcher.ViewModels;

public class SettingsViewModel(AppSettings appSettings) : ViewModelBase
{
    public SettingsGeneralViewModel SettingsGeneralViewModel { get; } = new(appSettings);
    public SettingsVersionsViewModel SettingsVersionsViewModel { get; } = new();
    public SettingsConfigsViewModel SettingsConfigsViewModel { get; } = new();
    
    public readonly AppSettings _appSettings = appSettings;
}
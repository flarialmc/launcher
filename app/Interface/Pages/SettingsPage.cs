using System.Windows;
using Flarial.Launcher.Management;
using Flarial.Launcher.Interface.Controls;
using ModernWpf.Controls;

namespace Flarial.Launcher.Interface.Pages;

sealed class SettingsPage : SimpleStackPanel
{
    readonly RadioButtons _dllBuild = new()
    {
        Header = "Select what DLL should be used:",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch
    };

    readonly ToggleSwitch _hardwareAcceleration = new()
    {
        Header = "Allow the launcher to use hardware acceleration?",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        OnContent = "Yes, improve launcher responsiveness.",
        OffContent = "No, fix any launcher graphical issues."
    };

    readonly ToggleSwitch _waitForInitialization = new()
    {
        Header = "Should the launcher wait for the game to initialize?",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        OnContent = "Yes, reduce game crashes at the cost of injection speed.",
        OffContent = "No, speed up injection with risk of game crashes."
    };

    readonly CustomDllPathPicker _customDllPath;

    internal SettingsPage(Configuration configuration)
    {
        Spacing = 12;
        Margin = new(12);

        _customDllPath = new(configuration);

        _dllBuild.Items.Add("Use the release DLL of the client which is stable.");
        _dllBuild.Items.Add("Use the beta DLL of the client which is unstable.");
        _dllBuild.Items.Add("Specify your own custom DLL to be used with the game.");

        Children.Add(_dllBuild);
        Children.Add(_customDllPath);
        Children.Add(_waitForInitialization);
        Children.Add(_hardwareAcceleration);

        _dllBuild.SelectionChanged += (_, _) =>
        {
            if (_dllBuild.SelectedIndex is -1) return;
            configuration.DllBuild = (DllBuild)_dllBuild.SelectedIndex;
            _customDllPath.IsEnabled = configuration.DllBuild is DllBuild.Custom;
        };

        _hardwareAcceleration.Toggled += (_, _) => configuration.HardwareAcceleration = _hardwareAcceleration.IsOn;
        _waitForInitialization.Toggled += (_, _) => configuration.WaitForInitialization = _waitForInitialization.IsOn;

        _dllBuild.SelectedIndex = (int)configuration.DllBuild;
        _hardwareAcceleration.IsOn = configuration.HardwareAcceleration;
        _waitForInitialization.IsOn = configuration.WaitForInitialization;
    }
}
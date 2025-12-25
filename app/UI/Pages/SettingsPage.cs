using System.Windows;
using Flarial.Launcher.App;
using Flarial.Launcher.UI.Controls;
using ModernWpf.Controls;

namespace Flarial.Launcher.UI.Pages;

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
        Header = "Configure whether the launcher should use hardware acceleration.",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch
    };

    readonly ToggleSwitch _waitForInitialization = new()
    {
        Header = "Configure whether the launcher should wait for the game to fully initialize.",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch
    };

    readonly CustomDllPathPicker _customDllPath;

    internal SettingsPage(Configuration configuration)
    {
        Spacing = 12;
        Margin = new(12);

        _customDllPath = new(configuration);

        _dllBuild.Items.Add("Release");
        _dllBuild.Items.Add("Beta");
        _dllBuild.Items.Add("Custom");

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
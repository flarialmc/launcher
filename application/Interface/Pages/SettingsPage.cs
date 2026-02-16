using System;
using System.Windows;
using System.Windows.Controls;
using Flarial.Launcher.Interface.Controls;
using Flarial.Launcher.Management;
using ModernWpf.Controls;

namespace Flarial.Launcher.Interface.Pages;

sealed class SettingsPage : Grid
{
    readonly Configuration _configuration;

    readonly RadioButtons _dllBuild = new()
    {
        Header = "Select what DLL should be used:",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch
    };

    readonly ToggleSwitch _waitForInitialization = new()
    {
        Header = "Should the launcher wait for the game to initialize?",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        OnContent = "Yes, reduce game crashes.",
        OffContent = "No, speed up injection."
    };

    readonly ToggleSwitch _automaticUpdates = new()
    {
        Header = "Should the launcher automatically update?",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        OnContent = "Yes, automatically update.",
        OffContent = "No, ask before updating."
    };

    void OnDllBuildSelectionChanged(object sender, EventArgs args)
    {
        if (((RadioButtons)sender).SelectedItem is FrameworkElement element)
        {
            _configuration.DllBuild = (Configuration.Build)element.Tag;
            _customDllPathPicker.IsEnabled = _configuration.DllBuild is Configuration.Build.Custom;
        }
    }

    void OnToggleSwitchToggled(object sender, EventArgs args)
    {
        var value = ((ToggleSwitch)sender).IsOn;
        if (ReferenceEquals(_automaticUpdates, sender)) _configuration.AutomaticUpdates = value;
        else if (ReferenceEquals(_waitForInitialization, sender)) _configuration.WaitForInitialization = value;
    }

    readonly CustomDllPathPicker _customDllPathPicker;

    internal SettingsPage(Configuration configuration)
    {
        _configuration = configuration;
        _customDllPathPicker = new(configuration);

        Margin = new(12);

        _dllBuild.SelectionChanged += OnDllBuildSelectionChanged;
        _dllBuild.Items.Add(new TextBlock { Tag = Configuration.Build.Release, Text = "Use the release DLL of the client which is stable." });
        _dllBuild.Items.Add(new TextBlock { Tag = Configuration.Build.Beta, Text = "Use the beta DLL of the client which is unstable." });
        _dllBuild.Items.Add(new TextBlock { Tag = Configuration.Build.Custom, Text = "Specify your own custom DLL to be used with the game." });

        _automaticUpdates.Toggled += OnToggleSwitchToggled;
        _waitForInitialization.Toggled += OnToggleSwitchToggled;

        _dllBuild.SelectedIndex = (int)configuration.DllBuild;
        _automaticUpdates.IsOn = configuration.AutomaticUpdates;
        _waitForInitialization.IsOn = configuration.WaitForInitialization;

        RowDefinitions.Add(new());
        RowDefinitions.Add(new() { Height = GridLength.Auto });

        SimpleStackPanel settingsPagePanel = new() { Spacing = 12 };
        SetColumn(settingsPagePanel, 0);
        SetRow(settingsPagePanel, 0);
        Children.Add(settingsPagePanel);

        SupportButtonsControl control = new();
        SetColumn(control, 0);
        SetRow(control, 1);
        Children.Add(control);

        settingsPagePanel.Children.Add(_dllBuild);
        settingsPagePanel.Children.Add(_customDllPathPicker);
        settingsPagePanel.Children.Add(_waitForInitialization);
        settingsPagePanel.Children.Add(_automaticUpdates);
    }
}
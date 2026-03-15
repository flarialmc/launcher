using System;
using Flarial.Launcher.Controls;
using Flarial.Launcher.Management;
using Flarial.Launcher.Xaml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Pages;


sealed class SettingsPage : Grid
{
    readonly ApplicationSettings _settings;

    readonly ToggleSwitch _automaticUpdates = new()
    {
        Header = "Should the launcher automatically update?",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        OnContent = "Yes, automatically update.",
        OffContent = "No, ask before updating."
    };

    void OnToggleSwitchToggled(object sender, RoutedEventArgs args)
    {
        var value = ((ToggleSwitch)sender).IsOn;
        if (ReferenceEquals(_automaticUpdates, sender)) _settings.AutomaticUpdates = value;
    }

    internal SettingsPage(ApplicationSettings settings)
    {
        _settings = settings;

        RowSpacing = 12;
        Margin = new(12);

        RowDefinitions.Add(new());
        RowDefinitions.Add(new() { Height = GridLength.Auto });

        StackPanel panel = new() { Spacing = 12, Orientation = Orientation.Vertical };
        FolderButtonsBox box = new();

        SetRow(panel, 0);
        SetColumn(panel, 0);

        SetRow(box, 1);
        SetColumn(box, 0);

        panel.Children.Add(_automaticUpdates);
        panel.Children.Add(new TextBlock { Text = "Select what DLL should be used:" });
        panel.Children.Add(new DllSelectionBox(settings));
        panel.Children.Add(new TextBlock { Text = "Select what initialization type should be used:" });
        panel.Children.Add(new InitializationTypeBox(settings));

        _automaticUpdates.Toggled += OnToggleSwitchToggled;
        _automaticUpdates.IsOn = _settings.AutomaticUpdates;

        Children.Add(panel);
        Children.Add(box);
    }
}
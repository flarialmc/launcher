using Flarial.Launcher.Controls;
using Flarial.Launcher.Management;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Pages;


sealed class SettingsPage : Grid
{
    readonly AppSettings _settings;

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

    internal SettingsPage(AppSettings settings)
    {
        _settings = settings;

        RowSpacing = 12;
        Margin = new(12);

        RowDefinitions.Add(new());
        RowDefinitions.Add(new() { Height = GridLength.Auto });

        FolderButtonsBox folderButtonsBox = new();
        StackPanel stackPanel = new() { Spacing = 12, Orientation = Orientation.Vertical };

        SetRow(stackPanel, 0);
        SetColumn(stackPanel, 0);

        SetRow(folderButtonsBox, 1);
        SetColumn(folderButtonsBox, 0);

        stackPanel.Children.Add(_automaticUpdates);
        stackPanel.Children.Add(new DllSelectionBox(settings));

        Children.Add(stackPanel);
        Children.Add(folderButtonsBox);

        _automaticUpdates.Toggled += OnToggleSwitchToggled;
        _automaticUpdates.IsOn = _settings.AutomaticUpdates;
    }
}
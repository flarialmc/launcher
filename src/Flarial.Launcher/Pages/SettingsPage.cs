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

    readonly ToggleSwitch _downloadVersions = new()
    {
        Header = "Should the launcher download first then install a version?",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        OnContent = "Yes, download first then install.",
        OffContent = "No, download & install at once."
    };

    readonly ToggleSwitch _waitForInitialization = new()
    {
        Header = "Should the launcher wait for the game to initialize?",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        OnContent = "Yes, reduce game crashes.",
        OffContent = "No, speed up injection."
    };

    void OnToggleSwitchToggled(object sender, RoutedEventArgs args)
    {
        var value = ((ToggleSwitch)sender).IsOn;
        if (ReferenceEquals(_automaticUpdates, sender)) _settings.AutomaticUpdates = value;
        else if (ReferenceEquals(_downloadVersions, sender)) _settings.DownloadVersions = value;
        else if (ReferenceEquals(_waitForInitialization, sender)) _settings.WaitForInitialization = value;
    }

    internal SettingsPage(AppSettings settings)
    {
        _settings = settings;

        RowSpacing = 12;
        Margin = new(12);

        RowDefinitions.Add(new());
        RowDefinitions.Add(new() { Height = GridLength.Auto });

        StackPanel stackPanel = new()
        {
            Spacing = 12,
            Orientation = Orientation.Vertical
        };
        FolderButtonsBox folderButtonsBox = new();

        SetRow(stackPanel, 0);
        SetColumn(stackPanel, 0);

        SetRow(folderButtonsBox, 1);
        SetColumn(folderButtonsBox, 0);

        stackPanel.Children.Add(_automaticUpdates);
        stackPanel.Children.Add(_downloadVersions);
        stackPanel.Children.Add(_waitForInitialization);
        stackPanel.Children.Add(new DllSelectionBox(settings));

        Children.Add(stackPanel);
        Children.Add(folderButtonsBox);

        _automaticUpdates.Toggled += OnToggleSwitchToggled;
        _waitForInitialization.Toggled += OnToggleSwitchToggled;
        _downloadVersions.Toggled += OnToggleSwitchToggled;

        _automaticUpdates.IsOn = _settings.AutomaticUpdates;
        _downloadVersions.IsOn = _settings.DownloadVersions;
        _waitForInitialization.IsOn = _settings.WaitForInitialization;
    }
}
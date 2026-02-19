using Flarial.Launcher.Controls;
using Flarial.Launcher.Management;
using Flarial.Launcher.Xaml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Pages;


sealed class SettingsPage : XamlElement<Grid>
{
    readonly ApplicationSettings _settings;

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

    void OnToggleSwitchToggled(object sender, RoutedEventArgs args)
    {
        var value = ((ToggleSwitch)sender).IsOn;
        if (ReferenceEquals(_automaticUpdates, sender)) _settings.AutomaticUpdates = value;
        else if (ReferenceEquals(_waitForInitialization, sender)) _settings.WaitForInitialization = value;
    }

    internal SettingsPage(ApplicationSettings settings) : base(new())
    {
        _settings = settings;

        _this.RowSpacing = 12;
        _this.Margin = new(12);

        _this.RowDefinitions.Add(new());
        _this.RowDefinitions.Add(new() { Height = GridLength.Auto });

        StackPanel panel = new() { Spacing = 12, Orientation = Orientation.Vertical };
        FolderButtonsBox box = new();

        Grid.SetRow(panel, 0);
        Grid.SetColumn(panel, 0);

        Grid.SetRow(box._this, 1);
        Grid.SetColumn(box._this, 0);

        panel.Children.Add(new TextBlock { Text = "Select what DLL should be used:" });
        panel.Children.Add(new DllSelectionBox(settings)._this);
        panel.Children.Add(_waitForInitialization);
        panel.Children.Add(_automaticUpdates);

        _automaticUpdates.Toggled += OnToggleSwitchToggled;
        _waitForInitialization.Toggled += OnToggleSwitchToggled;

        _automaticUpdates.IsOn = _settings.AutomaticUpdates;
        _waitForInitialization.IsOn = _settings.WaitForInitialization;

        _this.Children.Add(panel);
        _this.Children.Add(box._this);
    }
}
using System;
using Flarial.Launcher.Management;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Controls;

sealed class DllSelectionBox : Grid
{
    readonly CustomDllButton _button;
    readonly ApplicationSettings _settings;

    readonly ToggleSwitch _toggleSwitch = new()
    {
        Header = "Should the launcher use a custom DLL?",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        OnContent = "Yes, use a custom DLL.",
        OffContent = "No, use the client's DLL."
    };

    void OnToggleSwitchToggled(object sender, RoutedEventArgs args)
    {
        var value = ((ToggleSwitch)sender).IsOn;

        _button._button.IsEnabled = value;
        _button._textBox.IsEnabled = value;

        _settings.UseCustomDll = value;
    }

    internal DllSelectionBox(ApplicationSettings settings)
    {
        _settings = settings;
        _button = new(settings);

        RowSpacing = 12;
        ColumnSpacing = 12;

        RowDefinitions.Add(new());
        RowDefinitions.Add(new() { Height = GridLength.Auto });

        SetRow(_toggleSwitch, 0);
        SetColumn(_toggleSwitch, 0);

        SetRow(_button, 1);
        SetColumn(_button, 0);

        Children.Add(_toggleSwitch);
        Children.Add(_button);

        _toggleSwitch.Toggled += OnToggleSwitchToggled;
        _toggleSwitch.IsOn = _settings.UseCustomDll;
    }
}
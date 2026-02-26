using Flarial.Launcher.Management;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Controls;

sealed class DllSelectionBox : Grid
{
    readonly ApplicationSettings _settings;

    readonly ListBox _listBox = new()
    {
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch
    };

    readonly CustomDllPickerButton _button;

    void OnListBoxSelectionChanged(object sender, RoutedEventArgs args)
    {
        var listBox = (ListBox)sender;

        var item = (DllItem)listBox.SelectedItem;
        _settings.DllSelection = item.Value;

        var enabled = item.Value is DllSelection.Custom;
        _button._button.IsEnabled = enabled;
        _button._textBox.IsEnabled = enabled;
    }

    internal DllSelectionBox(ApplicationSettings settings)
    {
        _settings = settings;
        _button = new(_settings);

        RowDefinitions.Add(new());
        RowDefinitions.Add(new() { Height = GridLength.Auto });

        SetRow(_listBox, 0);
        SetColumn(_listBox, 0);

        SetRow(_button, 1);
        SetColumn(_button, 0);

        Children.Add(_listBox);
        Children.Add(_button);

        _listBox.Items.Add(new ClientDllItem());
        _listBox.Items.Add(new CustomDllItem());

        _listBox.SelectionChanged += OnListBoxSelectionChanged;

        _listBox.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
        VirtualizingStackPanel.SetVirtualizationMode(_listBox, VirtualizationMode.Recycling);

        _listBox.SelectedIndex = (int)_settings.DllSelection;
    }

    abstract class DllItem
    {
        protected abstract string String { get; }
        public override string ToString() => String;
        internal abstract DllSelection Value { get; }
    }

    sealed class ClientDllItem : DllItem
    {
        protected override string String => "Use the client's DLL with the game.";
        internal override DllSelection Value => DllSelection.Client;
    }

    sealed class CustomDllItem : DllItem
    {
        protected override string String => "Use a specified custom DLL with the game.";
        internal override DllSelection Value => DllSelection.Custom;
    }
}
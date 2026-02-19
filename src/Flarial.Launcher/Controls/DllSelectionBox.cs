using Flarial.Launcher.Management;
using Flarial.Launcher.Xaml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Controls;

file abstract class DllItem
{
    protected abstract string String { get; }
    public override string ToString() => String;
    internal abstract DllSelection Value { get; }
}

file sealed class ReleaseDllItem : DllItem
{
    protected override string String => "Use the release DLL of the client which is stable.";
    internal override DllSelection Value => DllSelection.Release;
}

file sealed class BetaDllItem : DllItem
{
    protected override string String => "Use the beta DLL of the client which is unstable.";
    internal override DllSelection Value => DllSelection.Beta;
}

file sealed class CustomDllItem : DllItem
{
    protected override string String => "Specify your own custom DLL to be used with the game.";
    internal override DllSelection Value => DllSelection.Custom;
}

sealed class DllSelectionBox : XamlElement<Grid>
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

    internal DllSelectionBox(ApplicationSettings settings) : base(new())
    {
        _settings = settings;
        _button = new(_settings);

        @this.RowDefinitions.Add(new());
        @this.RowDefinitions.Add(new() { Height = GridLength.Auto });

        Grid.SetRow(_listBox, 0);
        Grid.SetColumn(_listBox, 0);

        Grid.SetRow(_button.@this, 1);
        Grid.SetColumn(_button.@this, 0);

        @this.Children.Add(_listBox);
        @this.Children.Add(_button.@this);

        _listBox.Items.Add(new ReleaseDllItem());
        _listBox.Items.Add(new BetaDllItem());
        _listBox.Items.Add(new CustomDllItem());

        _listBox.SelectionChanged += OnListBoxSelectionChanged;

        _listBox.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
        VirtualizingStackPanel.SetVirtualizationMode(_listBox, VirtualizationMode.Recycling);

        _listBox.SelectedIndex = (int)_settings.DllSelection;
    }
}
using System;
using Flarial.Launcher.Management;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Controls;

sealed class InitializationTypeBox : Grid
{
    readonly ApplicationSettings _settings;

    readonly ListBox _listBox = new()
    {
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch
    };

    void OnListBoxSelectionChanged(object sender, RoutedEventArgs args)
    {
        var item = ((ListBox)sender).SelectedItem;
        _settings.WaitForInitialization = ((ContentItem<bool?>)item).Value;
    }

    internal InitializationTypeBox(ApplicationSettings settings)
    {
        _settings = settings;

        RowDefinitions.Add(new());
        RowDefinitions.Add(new() { Height = GridLength.Auto });

        SetRow(_listBox, 0);
        SetColumn(_listBox, 0);

        Children.Add(_listBox);

        _listBox.Items.Add(new WaitForWindow());
        _listBox.Items.Add(new WaitForTitleScreen());
        _listBox.Items.Add(new WaitForGlobalResources());

        _listBox.SelectionChanged += OnListBoxSelectionChanged;
        _listBox.SelectedIndex = _settings.WaitForInitialization switch { null => 0, false => 1, true => 2 };

        _listBox.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
        VirtualizingStackPanel.SetVirtualizationMode(_listBox, VirtualizationMode.Recycling);
    }

    sealed class WaitForWindow : ContentItem<bool?>
    {
        internal override bool? Value => null;
        protected override string String => "Wait for window, unsafe & fast.";
    }

    sealed class WaitForTitleScreen : ContentItem<bool?>
    {
        internal override bool? Value => false;
        protected override string String => "Wait for title screen, risky & moderate.";
    }

    sealed class WaitForGlobalResources : ContentItem<bool?>
    {
        internal override bool? Value => true;
        protected override string String => "Wait for global resources, safe & slow.";
    }

}
using System.Windows;
using System.Windows.Controls;
using Flarial.Launcher.Interface;
using Flarial.Launcher.Management;
using Microsoft.Win32;
using ModernWpf.Controls;

namespace Flarial.Launcher.Interface.Controls;

sealed class CustomDllPathPicker : SimpleStackPanel
{
    readonly TextBox _textBox = new()
    {
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        IsHitTestVisible = default,
        IsInactiveSelectionHighlightEnabled = default,
        IsReadOnly = true
    };

    readonly Button _button = new()
    {
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Content = new SymbolIcon(Symbol.OpenFile),
        Margin = new(0, 0, 12, 0)
    };

    readonly OpenFileDialog _dialog = new()
    {
        CheckFileExists = true,
        CheckPathExists = true,
        DereferenceLinks = true,
        Filter = "Dynamic-Link Libraries (*.dll)|*.dll"
    };

    internal CustomDllPathPicker(Configuration configuration)
    {
        VerticalAlignment = VerticalAlignment.Stretch;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        IsEnabled = default;
        Spacing = 12;

        Children.Add(new TextBlock { Text = "Select a custom DLL:" });

        Grid grid = new();
        Children.Add(grid);

        grid.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new());

        _textBox.Text = configuration.CustomDllPath;

        Grid.SetRow(_button, 0);
        Grid.SetColumn(_button, 0);
        grid.Children.Add(_button);

        Grid.SetRow(_textBox, 0);
        Grid.SetColumn(_textBox, 1);
        grid.Children.Add(_textBox);

        _button.Click += (_, _) =>
        {
            if (_dialog.ShowDialog() is not { } @_ || !@_)
                return;

            _textBox.Text = _dialog.FileName;
            configuration.CustomDllPath = _dialog.FileName;
        };
    }
}
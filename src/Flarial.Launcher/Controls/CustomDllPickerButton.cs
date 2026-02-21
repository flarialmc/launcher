using Flarial.Launcher.Management;
using Flarial.Launcher.Xaml;
using Microsoft.Win32;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flarial.Launcher.Controls;

sealed class CustomDllPickerButton : XamlElement<Grid>
{
    readonly OpenFileDialog _dialog = new()
    {
        ValidateNames = true,
        DereferenceLinks = true,
        CheckFileExists = true,
        CheckPathExists = true,
        ReadOnlyChecked = true,
        RestoreDirectory = false,
        Filter = "Dynamic-Link Libraries (*.dll)|*.dll"
    };

    internal readonly Button _button = new()
    {
        Content = new SymbolIcon(Symbol.OpenFile),
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        IsEnabled = false
    };

    internal readonly TextBox _textBox = new()
    {
        IsReadOnly = true,
        IsEnabled = false,
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch
    };

    void OnButtonClick(object sender, RoutedEventArgs args)
    {
        if (_dialog.ShowDialog() is { } @_ && @_)
        {
            _textBox.Text = _dialog.FileName;
            _settings.CustomDllPath = _dialog.FileName;
        }
    }

    readonly ApplicationSettings _settings;

    internal CustomDllPickerButton(ApplicationSettings settings) : base(new())
    {
        _settings = settings;

        @this.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        @this.ColumnDefinitions.Add(new());

        Grid.SetRow(_button, 0);
        Grid.SetColumn(_button, 0);

        Grid.SetRow(_textBox, 0);
        Grid.SetColumn(_textBox, 1);

        @this.Children.Add(_button);
        @this.Children.Add(_textBox);

        _button.Click += OnButtonClick;

        _textBox.Text = settings.CustomDllPath;
    }
}
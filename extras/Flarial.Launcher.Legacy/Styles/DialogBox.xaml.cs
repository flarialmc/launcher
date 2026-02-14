using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Flarial.Launcher.Animations;
using Flarial.Launcher.Pages;
using Windows.UI.Notifications;

namespace Flarial.Launcher.Styles;

public partial class DialogBox : UserControl
{
    private static TextBlock _titleTextBlock;
    private static TextBlock _descriptionTextBlock;
    private static StackPanel _panel;

    public DialogBox()
    {
        InitializeComponent();
        _titleTextBlock = TitleTextBlock;
        _descriptionTextBlock = DescriptionTextBlock;
        _panel = ButtonsStackPanel;
    }

    static readonly SolidColorBrush s_primary = new(Color.FromRgb(0xFF, 0x24, 0x38));

    static readonly SolidColorBrush s_secondary = new(Color.FromRgb(0x3F, 0x2A, 0x2D));

    public static async Task<T> ShowAsync<T>(string title, string description, params (string, T)[] buttons)
    {
        if (buttons.Length < 0) throw new ArgumentException();

        TaskCompletionSource<T> source = new();

        DialogAnimations.OpenDialog(SettingsPage.MainGrid, MainWindow.MainWindowDialogBox);
        _panel.Children.Clear();

        _titleTextBlock.Text = title;
        _descriptionTextBlock.Text = description;

        for (var index = 0; index < buttons.Length; index++)
        {
            var (content, result) = buttons[index];

            Button button = new()
            {
                Content = content,
                Background = index is 0 ? s_primary : s_secondary
            };

            button.Click += delegate
            {
                DialogAnimations.CloseDialog(SettingsPage.MainGrid, MainWindow.MainWindowDialogBox);
                MainWindow.MainWindowDialogBox.Visibility = Visibility.Collapsed;
                source.TrySetResult(result);
            };

            _panel.Children.Add(button);
        }

        return await source.Task;
    }

    [Obsolete("Directly obtain button data from the caller instead of relying on an 'enum'.", true)]
    public static async Task<TEnum> ShowAsync<TEnum>(string title, string description) where TEnum : Enum
    {
        var tcs = new TaskCompletionSource<TEnum>();

        DialogAnimations.OpenDialog(SettingsPage.MainGrid, MainWindow.MainWindowDialogBox);
        _panel.Children.Clear();

        var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();

        for (var index = 0; index < values.Count; index++)
        {
            var value = values[index];
            var button = new Button
            {
                Content = AddSpacesBeforeCapitals(value.ToString()),
                Background = index == 0 ? new SolidColorBrush(Color.FromRgb(0xff, 0x24, 0x38)) : new SolidColorBrush(Color.FromRgb(0x3f, 0x2a, 0x2d))
            };

            var value1 = value;
            button.Click += (_, _) =>
            {
                DialogAnimations.CloseDialog(SettingsPage.MainGrid, MainWindow.MainWindowDialogBox);
                tcs.TrySetResult(value1);
                MainWindow.MainWindowDialogBox.Visibility = Visibility.Collapsed;
            };

            _panel.Children.Add(button);
        }

        return await tcs.Task;
    }

    [Obsolete("Directly obtain button data from the caller instead of relying on an 'enum'.", true)]
    private static string AddSpacesBeforeCapitals(string input)
    {
        return string.IsNullOrWhiteSpace(input) ? input : Regex.Replace(input, "(?<!^)([A-Z])", " $1");
    }
}
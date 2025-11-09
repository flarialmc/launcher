using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Flarial.Launcher.Animations;
using Flarial.Launcher.Pages;

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
                Background = index == 0 ? new SolidColorBrush(Color.FromRgb(0xff, 0x24,  0x38)) : new SolidColorBrush(Color.FromRgb(0x3f, 0x2a, 0x2d ))
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
    
    private static string AddSpacesBeforeCapitals(string input)
    {
        return string.IsNullOrWhiteSpace(input) ? input : Regex.Replace(input, "(?<!^)([A-Z])", " $1");
    }
}
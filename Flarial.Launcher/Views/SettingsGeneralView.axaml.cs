using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Flarial.Launcher.ViewModels;

namespace Flarial.Launcher.Views;

public partial class SettingsGeneralView : UserControl
{
    public SettingsGeneralView()
    {
        InitializeComponent();
        DataContext = new SettingsGeneralViewModel();
    }
}
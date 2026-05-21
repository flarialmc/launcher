using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Flarial.Launcher.ViewModels;

namespace Flarial.Launcher.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        //DataContext = new HomeViewModel();
        
        
    }

    private void LaunchButton_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }
}
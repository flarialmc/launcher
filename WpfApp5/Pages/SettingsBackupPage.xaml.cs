using System.Windows;
using System.Windows.Controls;
using Flarial.Launcher.Styles;

namespace Flarial.Launcher.Pages;

public partial class SettingsBackupPage : Page
{
    private static VirtualizingStackPanel _stackPanel;
    
    public SettingsBackupPage()
    {
        InitializeComponent();

        _stackPanel = BackupStackPanel;
    }

    public static void AddBackupItem(string time, string path) => _stackPanel.Children.Add(new BackupItem{Time = time, Path = path});

    //just placeholder stuff
    private void SettingsBackupPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        for (var i = 0; i < 25; i++)
        {
            SettingsBackupPage.AddBackupItem("9/11", "idk");
        }
    }
}
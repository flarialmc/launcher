using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using Flarial.Launcher.Animations;
using Flarial.Launcher.Functions;

namespace Flarial.Launcher.Pages;

/// <summary>
/// Interaction logic for SettingsPage.xaml
/// </summary>
public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        GeneralPageButton.IsChecked = true;
    }

    public static Border b1;
    public static Grid MainGrid;

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        => SettingsPageTransition.SettingsLeaveAnimation(b1, MainGrid);

    private void Navigate_General(object sender, RoutedEventArgs e)
        => SettingsPageTransition.SettingsNavigateAnimation(0, PageBorder, PageStackPanel);

    private void Navigate_Version(object sender, RoutedEventArgs e)
    {
        if (!SDK.Minecraft.Installed)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.CreateMessageBox("Minecraft isn't installed, please it!");
            });
            return;
        }

        if (!Utils.IsAdministrator)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.CreateMessageBox("Please restart & run the launcher as an adminstrator to change versions!");
            });
            return;
        }

        SettingsPageTransition.SettingsNavigateAnimation(-500, PageBorder, PageStackPanel);
        Application.Current.Dispatcher.Invoke(() =>
         {
             MainWindow.CreateMessageBox("This a version changer not a version switcher or selector.");
         });
    }

    private void Navigate_Account(object sender, RoutedEventArgs e)
        => SettingsPageTransition.SettingsNavigateAnimation(-1000, PageBorder, PageStackPanel);
    private void Navigate_Backups(object sender, RoutedEventArgs e)

        => SettingsPageTransition.SettingsNavigateAnimation(-1500, PageBorder, PageStackPanel);
}

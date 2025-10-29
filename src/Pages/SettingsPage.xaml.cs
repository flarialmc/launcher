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
    static bool s_shown = false;

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        => SettingsPageTransition.SettingsLeaveAnimation(b1, MainGrid);

    private void Navigate_General(object sender, RoutedEventArgs e)
    {
        SettingsPageTransition.SettingsNavigateAnimation(0, PageBorder, PageStackPanel);
    }

    internal void Navigate_Versions(object sender, RoutedEventArgs e)
    {
        if (!SDK.Minecraft.Installed)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.CreateMessageBox("Please install Minecraft from the Microsoft Store or Xbox App.");
            });
            return;
        }

        if (SDK.Minecraft.Unpackaged && !Utils.IsAdministrator)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.CreateMessageBox("Run the launcher as an administrator to downgrade or update versions.");
            });
            return;
        }

        if (SDK.Minecraft.GDK && !s_shown)
        {
            s_shown = true;
            MainWindow.CreateMessageBox("⚠️ Backups will not be generated for the GDK builds of the game.");
        }

        SettingsPageTransition.SettingsNavigateAnimation(-500, PageBorder, PageStackPanel);
    }

    private void Navigate_Account(object sender, RoutedEventArgs e)
    {
        SettingsPageTransition.SettingsNavigateAnimation(-1000, PageBorder, PageStackPanel);
    }

    private void Navigate_Backups(object sender, RoutedEventArgs e)
    {
        SettingsPageTransition.SettingsNavigateAnimation(-1500, PageBorder, PageStackPanel);
    }
}

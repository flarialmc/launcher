using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using Flarial.Launcher.Animations;
using Flarial.Launcher.Functions;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Styles;

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

    internal async void Navigate_Versions(object sender, RoutedEventArgs e)
    {
        if (!Minecraft.IsInstalled)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.CreateMessageBox("Please install Minecraft from the Microsoft Store or Xbox App.");
            });
            return;
        }

        if (Minecraft.IsUnpackaged && !Utils.IsAdministrator)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.CreateMessageBox("Run the launcher as an administrator to downgrade or update versions.");
            });
            return;
        }

        if (Minecraft.UsingGameDevelopmentKit && !s_shown)
        {
            s_shown = await DialogBox.ShowAsync("🚨 GDK Backups Unsupported", @"A GDK build of the game is currently installed. 
The launcher doesn't support backing up data for GDK builds so please backup any data manually beforehand to avoid data loss.", ("OK", true));
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

using System.Windows;
using System.Windows.Controls;
using Flarial.Launcher.Animations;
using Utils = Flarial.Launcher.Functions.Utils;

namespace Flarial.Launcher.Pages
{
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
            if (Utils.IsAdministrator)
            {
                SettingsPageTransition.SettingsNavigateAnimation(-500, PageBorder, PageStackPanel);
                Application.Current.Dispatcher.Invoke(() =>
                 {
                     MainWindow.CreateMessageBox("This a version changer not a version switcher or selector.");
                 });
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.CreateMessageBox("Launcher not running as Administrator. Run as Admin!");
                });
            }
        }

        private void Navigate_Account(object sender, RoutedEventArgs e)
            => SettingsPageTransition.SettingsNavigateAnimation(-1000, PageBorder, PageStackPanel);
        private void Navigate_Backups(object sender, RoutedEventArgs e)

            => SettingsPageTransition.SettingsNavigateAnimation(-1500, PageBorder, PageStackPanel);
    }
}

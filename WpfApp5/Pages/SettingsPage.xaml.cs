using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Flarial.Launcher.Animations;

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

        public static Border? b1;
        public static Grid? MainGrid;

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
            => SettingsPageTransition.SettingsLeaveAnimation(b1, MainGrid);

        private void Navigate_General(object sender, RoutedEventArgs e)
            => SettingsPageTransition.SettingsNavigateAnimation(0, PageBorder, PageStackPanel);
        private void Navigate_Version(object sender, RoutedEventArgs e)
        {
            SettingsPageTransition.SettingsNavigateAnimation(-500, PageBorder, PageStackPanel);
            MessageBox.Show("!!! THIS IS A VERSION CHANGER. Not a Flarial Version SELECTOR! Use this when you need to downgrade to a version Flarial Supports. !!!\n To use Flarial, all you have to do is click Launch.", "MUST READ", MessageBoxButton.OK, MessageBoxImage.Exclamation);

        }
        private void Navigate_Account(object sender, RoutedEventArgs e)
            => SettingsPageTransition.SettingsNavigateAnimation(-1000, PageBorder, PageStackPanel);
    }
}

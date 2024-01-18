using Flarial.Launcher.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Flarial.Launcher.Pages
{
    /// <summary>
    /// Interaction logic for SettingsGeneralPage.xaml
    /// </summary>
    public partial class SettingsGeneralPage : Page
    {
        public static ToggleButton saveButton;

        public SettingsGeneralPage()
        {
            InitializeComponent();
            saveButton = SaveButton;
            tb1.IsChecked = Config.UseCustomDLL;
            tb2.IsChecked = Config.UseBetaDLL;
            tb3.IsChecked = Config.AutoLogin;
            tb4.IsChecked = Config.CloseToTray;
            DLLTextBox.Text = Config.CustomDLLPath;
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        => Animations.ToggleButtonTransitions.CheckedAnimation(DllGrid);

        private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
            => Animations.ToggleButtonTransitions.UnCheckedAnimation(DllGrid);

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CreateMessageBox("This feature is currently unavalible");
            ((ToggleButton)sender).IsChecked = false;
        }

        private void ToggleButton_Click_1(object sender, RoutedEventArgs e)
        {
            Config.AutoLogin = (bool)((ToggleButton)sender).IsChecked;
            SaveButton.IsChecked = true;
        }

        private void ToggleButton_Click_2(object sender, RoutedEventArgs e)
        {
            Config.UseCustomDLL = (bool)((ToggleButton)sender).IsChecked;
            SaveButton.IsChecked = true;
        }

        private void ToggleButton_Click_3(object sender, RoutedEventArgs e)
        {
            Config.CloseToTray = (bool)((ToggleButton)sender).IsChecked;
            SaveButton.IsChecked = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Config.saveConfig();
            SaveButton.IsChecked = false;
        }
    }
}
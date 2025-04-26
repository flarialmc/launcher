using Bedrockix.Minecraft;
using Flarial.Launcher.Functions;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Flarial.Launcher.Pages;

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

            tb1.Checked += (_, _) =>
            {
                if ((bool)tb2.IsChecked)
                {
                    tb2.IsChecked = false;
                    Config.UseBetaDLL= false;
                }
            };

            tb2.Checked += (_, _) =>
            {
                if ((bool)tb1.IsChecked)
                {
                    tb1.IsChecked = false;
                    Config.UseCustomDLL = false;
                }
            };

            ((MainWindow)Application.Current.MainWindow).HomePage.IsEnabledChanged += (_, _) =>
            {
                if (Config.UseCustomDLL && Config.UseBetaDLL)
                    Config.UseCustomDLL = Config.UseBetaDLL = false;

                tb1.IsChecked = Config.UseCustomDLL;
                tb2.IsChecked = Config.UseBetaDLL;
                tb3.IsChecked = Config.AutoLogin;
                tb4.IsChecked = Config.MCMinimized;
                DLLTextBox.Value = Config.CustomDLLPath;
            };
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        => Animations.ToggleButtonTransitions.CheckedAnimation(DllGrid);

        private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
            => Animations.ToggleButtonTransitions.UnCheckedAnimation(DllGrid);

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            Config.UseBetaDLL = (bool)((ToggleButton)sender).IsChecked;
            SaveButton.IsChecked = true;
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
            Config.MCMinimized = (bool)((ToggleButton)sender).IsChecked;
            SaveButton.IsChecked = true;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => Config.SaveConfig());
            SaveButton.IsChecked = false;
        }
    }
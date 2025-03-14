﻿using Flarial.Launcher.Functions;
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
            tb4.IsChecked = Config.MCMinimized;
            DLLTextBox.Text = Config.CustomDLLPath;
            ModulesSlider.Value = Math.Round(Config.WaitFormodules);
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
            MainWindow.CreateMessageBox("Restart the Launcher to see the changes.");
            SaveButton.IsChecked = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => Config.saveConfig());
            SaveButton.IsChecked = false;
        }

        private void ModulesSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ModulesSlider.Value = Math.Round(ModulesSlider.Value);
            Config.WaitFormodules = ModulesSlider.Value;
            if(SaveButton != null)
            SaveButton.IsChecked = true;
        }
    }
}
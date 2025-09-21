﻿using Flarial.Launcher.Functions;
using Flarial.Launcher.Pages;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace Flarial.Launcher.Styles
{
    /// <summary>
    /// Interaction logic for TextBox.xaml
    /// </summary>
    public partial class TextBox : UserControl
    {
        private readonly Settings _settings = Settings.Current;
        
        public TextBox()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string Value { get => textbox.Text; set => textbox.Text = value; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new();
            dialog.InitialDirectory = @"C:\";
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Multiselect = false;

            dialog.DefaultExt = "dll";
            dialog.Filter = "DLL Files|*.dll;";
            dialog.Title = "Select Custom DLL";
            if (dialog.ShowDialog() != true) return;
            textbox.Text = dialog.FileName;
            _settings.CustomDllPath = dialog.FileName;
            SettingsGeneralPage.saveButton.IsChecked = true;
        }

        private void Textbox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _settings.CustomDllPath = textbox.Text;
        }
    }
}

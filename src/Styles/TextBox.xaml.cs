using Flarial.Launcher.Functions;
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
        public TextBox()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string Value { get => textbox.Text; set => textbox.Text = value; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                InitialDirectory = @"C:\",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,

                DefaultExt = "dll",
                Filter = "DLL Files|*.dll;",
                Title = "Select Custom DLL"
            };
            if (dialog.ShowDialog() == true)
            {
                textbox.Text = dialog.FileName;
                Config.CustomDllPath = dialog.FileName;
                SettingsGeneralPage.SaveConfigButton.IsChecked = true;
            }
        }
    }
}

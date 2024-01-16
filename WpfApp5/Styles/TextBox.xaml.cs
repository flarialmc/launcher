using Flarial.Launcher.Functions;
using Flarial.Launcher.Pages;
using Microsoft.Win32;
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

        public string Text { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = @"C:\";
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Multiselect = false;

            dialog.DefaultExt = "dll";
            dialog.Filter = "DLL Files|*.dll;";
            dialog.Title = "Select Custom DLL";
            if (dialog.ShowDialog() == true)
            {
                textbox.Text = dialog.FileName;
                Config.CustomDLLPath = dialog.FileName;
                SettingsGeneralPage.saveButton.IsChecked = true;
            }
        }
    }
}

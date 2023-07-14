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

namespace Flarial.Launcher
{
    /// <summary>
    /// Interaction logic for CustomTextBox.xaml
    /// </summary>
    public partial class CustomTextBox : UserControl
    {
        public CustomTextBox()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string Type { get; set; }
        public string Text { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = @"C:\";
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Multiselect = false;

            if (Type == "dll")
            {
                dialog.DefaultExt = "dll";
                dialog.Filter = "DLL Files|*.dll;";
                dialog.Title = "Select Custom DLL";
                if (dialog.ShowDialog() == true)
                {
                    textbox.Text = dialog.FileName;
                    MainWindow.custom_dll_path = dialog.FileName;
                }
            }
            if (Type == "theme")
            {
                dialog.DefaultExt = "xaml";
                dialog.Filter = "Flarial Themes|*.xaml;";
                dialog.Title = "Select Custom Theme";
                if (dialog.ShowDialog() == true)
                {
                    textbox.Text = dialog.FileName;
                    MainWindow.custom_theme_path = dialog.FileName;
                }
            }
        }
    }
}

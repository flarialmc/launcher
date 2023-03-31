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
using System.Windows.Shapes;

namespace Flarial.Launcher
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class CustomDialogBox : Window
    {
        public CustomDialogBox(string Title, string Question, string type = "DialogBox")
        {
            InitializeComponent();
            MessageBoxText.Text = Question;
            MessageBoxTitle.Content = Title;
            if (type == "MessageBox")
            {
                MessageBoxOk.Visibility = Visibility.Visible;
                MessageBoxYes.Visibility = Visibility.Hidden;
                MessageBoxNo.Visibility = Visibility.Hidden;
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MessageBoxYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}

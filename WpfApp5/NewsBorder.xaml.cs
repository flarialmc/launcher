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

namespace Flarial.NewsPanel
{
    /// <summary>
    /// Interaction logic for NewsBorder.xaml
    /// </summary>
    public partial class NewsBorder : UserControl
    {
        public NewsBorder()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string Title { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public string RoleName { get; set; }
        public string RoleColor { get; set; }
        public string AuthorAvatar { get; set; }
        public string Date { get; set; }
        public string Background1 { get; set; }
    }
}

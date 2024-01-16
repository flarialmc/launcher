using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for NewsItem.xaml
    /// </summary>
    public partial class NewsItem : UserControl
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public string RoleName { get; set; }
        public string RoleColor { get; set; }
        public string BackgroundURL { get; set; }
        public string AuthorAvatar { get; set; }
        public string Date { get; set; }

        public NewsItem()
        {
            InitializeComponent();
            DataContext = this;
        }
    }

    [ValueConversion(typeof(String), typeof(BitmapImage))]
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string fullFilePath = value.ToString();

            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
            bitmap.EndInit();

            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }

    [ValueConversion(typeof(String), typeof(SolidColorBrush))]
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return (SolidColorBrush)new BrushConverter().ConvertFrom(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}

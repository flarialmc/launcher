using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Flarial.Launcher.Managers;
using Flarial.Launcher.Pages;

namespace Flarial.Launcher.Styles
{
    /// <summary>
    /// Interaction logic for VersionItem.xaml
    /// </summary>

    public partial class VersionItem : RadioButton
    {
        public string verlink = "";

        public VersionItem()
        {
            DataContext = new VersionItemProperties();
            InitializeComponent();
        }

        private async void VersionItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (MainWindow.isDownloadingVersion) return;

            if (VersionItemProperties.GetState(this) == 0)
            {
                foreach (VersionItem item in SettingsVersionPage.sp.Children)
                {
                    item.IsEnabled = false;
                }
                IsEnabled = true;

                long time = 0;
                string version = VersionItemProperties.GetVersion(this);

                DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(50) };
                timer.Tick += (s, _e) =>
                {
                    VersionItemProperties.SetInstallPercentage(this, MainWindow.progressPercentage.ToString());


                    time += 50;

                    if (Minecraft.isInstalled() && time > 7000 && MainWindow.progressPercentage == 100 && MainWindow.progressType == "Installing")
                    {
                        Trace.WriteLine("yes 1");
                        timer.Stop();
                        VersionItemProperties.SetState(this, 3);
                        foreach (VersionItem item in SettingsVersionPage.sp.Children)
                        {
                            item.IsEnabled = true;
                        }
                        MainWindow.versionLabel.Text = version;
                    }
                };

                timer.Start();

                VersionItemProperties.SetState(this, 1);

                bool succeeded = await Task.Run(() => VersionManagement.InstallMinecraft(version, this));
                if (!succeeded)
                {
                    IsChecked = false;
                    VersionItemProperties.SetState(this, 2);
                }
                else
                {

                    foreach (VersionItem vi in SettingsVersionPage.sp.Children)
                    {
                        if (VersionItemProperties.GetVersion(vi) != version)
                        {
                            VersionItemProperties.SetState(vi, 0);
                        }
                    }
                    MainWindow.progressPercentage = 100;
                }
            }
            else if (VersionItemProperties.GetState(this) == 2)
            {
                string version = VersionItemProperties.GetVersion(this);
                foreach (VersionItem item in SettingsVersionPage.sp.Children)
                {
                    item.IsEnabled = false;

                    if (VersionItemProperties.GetVersion(item) != version && VersionItemProperties.GetState(item) == 3)
                    {
                        VersionItemProperties.SetState(item, 2);
                    }
                }
                IsEnabled = true;

                long time = 0;

                DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(50) };
                timer.Tick += (object s, EventArgs _e) =>
                {
                    double acc = 0;
                    if (MainWindow.progressType == "download")
                        acc = MainWindow.progressBytesReceived;
                    else if (MainWindow.progressType == "Extracting")
                        acc = MainWindow.progressBytesReceived / MainWindow.progressBytesTotal;
                    else if (MainWindow.progressType == "Installing")
                        acc = 99;
                    else if (MainWindow.progressType == "backup")
                        acc = 0;

                    VersionItemProperties.SetInstallPercentage(this, acc.ToString());


                    time += 50;

                    if (Minecraft.isInstalled() && time > 7000 && MainWindow.progressPercentage == 100 && MainWindow.progressType == "Installing")
                    {
                        Trace.WriteLine("yes 1");
                        timer.Stop();
                        VersionItemProperties.SetState(this, 3);
                        foreach (VersionItem item in SettingsVersionPage.sp.Children)
                        {
                            item.IsEnabled = true;
                        }
                        MainWindow.versionLabel.Text = version;
                    }
                };

                timer.Start();

                VersionItemProperties.SetState(this, 1);
                bool succeeded = await Task.Run(() => VersionManagement.InstallMinecraft(version, this));
                if (!succeeded)
                {
                    IsChecked = false;
                    VersionItemProperties.SetState(this, 2);
                }
                else
                {
                    MainWindow.progressPercentage = 100;
                    VersionItemProperties.SetState(this, 3);
                }
            }
        }

        private void VersionItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            VersionItemProperties.SetState(this, 2);
        }
    }

    public class InstallGridConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double x = 0;
            if (!string.IsNullOrEmpty((string)values[0]))
                x = System.Convert.ToDouble((string)values[0]);

            double y = (double)values[1];

            return y - x / 100 * y;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Going back to what you had isn't supported.");
        }
    }

    public class VersionItemProperties
    {
        public static readonly DependencyProperty StateProperty;

        public static readonly DependencyProperty VersionProperty;

        public static readonly DependencyProperty VersionLinkProperty;

        public static readonly DependencyProperty InstallPercentageProperty;

        public static readonly DependencyProperty ImageURLProperty;

        static VersionItemProperties()
        {
            StateProperty = DependencyProperty.RegisterAttached("State", typeof(int), typeof(VersionItemProperties));

            VersionProperty = DependencyProperty.RegisterAttached("Version", typeof(string), typeof(VersionItemProperties));

            VersionLinkProperty = DependencyProperty.RegisterAttached("VersionLink", typeof(string), typeof(VersionItemProperties));

            InstallPercentageProperty = DependencyProperty.RegisterAttached("InstallPercentage", typeof(string), typeof(VersionItemProperties));

            ImageURLProperty = DependencyProperty.RegisterAttached("ImageURL", typeof(BitmapImage), typeof(VersionItemProperties));
        }

        public static void SetState(UIElement element, int value) => element.SetValue(StateProperty, value);

        public static int GetState(UIElement element) => (int)element.GetValue(StateProperty);

        public static void SetVersion(UIElement element, string value) => element.SetValue(VersionProperty, value);

        public static void SetVersionLink(UIElement element, string value) => element.SetValue(VersionLinkProperty, value);

        public static string GetVersion(UIElement element) => (string)element.GetValue(VersionProperty);

        public static string GetVersionLink(UIElement element) => (string)element.GetValue(VersionLinkProperty);

        public static void SetInstallPercentage(UIElement element, string value) => element.SetValue(InstallPercentageProperty, value);

        public static string GetInstallPercentage(UIElement element) => (string)element.GetValue(InstallPercentageProperty);

        public static void SetImageURL(UIElement element, BitmapImage value) => element.SetValue(ImageURLProperty, value);

        public static BitmapImage GetImageURL(UIElement element) => (BitmapImage)element.GetValue(ImageURLProperty);
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            /*
            if(!Minecraft.StoreHelper.HasBought().Result)
            {
                MainWindow.CreateMessageBox("You haven't bought Minecraft! We cannot switch your Minecraft Version.");
                MainWindow.CreateMessageBox("You have to update manually.");
            }*/
            if (VersionItemProperties.GetState(this) == 0)
            {
                foreach (VersionItem item in SettingsVersionPage.sp.Children)
                {
                    item.IsEnabled = false;
                }
                this.IsEnabled = true;

                long time = 0;
                string version = VersionItemProperties.GetVersion(this);

                DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
                timer.Tick += (object s, EventArgs _e) =>
                {
                    /*double acc = 0;
                    if (MainWindow.progressType == "download")
                        acc = MainWindow.progressBytesReceived;
                    else if (MainWindow.progressType == "Extracting")
                        acc = MainWindow.progressBytesReceived / MainWindow.progressBytesTotal;
                    else if (MainWindow.progressType == "Installing")
                        acc = 75;
                    else if (MainWindow.progressType == "backup")
                        acc = 90;*/

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

                string link = VersionItemProperties.GetVersionLink(this);
                bool succeeded = await Task.Run(() => VersionManagement.InstallMinecraft(link, version, this));
                if (!succeeded)
                {
                    this.IsChecked = false;
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
                this.IsEnabled = true;

                long time = 0;

                DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
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
                string link = (await MainWindow.VersionCatalog.UriAsync(version)).OriginalString;
                bool succeeded = await Task.Run(() => VersionManagement.InstallMinecraft(link, version, this));
                if (!succeeded)
                {
                    this.IsChecked = false;
                    VersionItemProperties.SetState(this, 2);
                }
                else
                {
                    MainWindow.progressPercentage = 100;
                    VersionItemProperties.SetState(this, 3);
                }
            }
        }

        /*private async void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            object[] tags = (object[])radioButton.Tag;
            string version = tags[1].ToString();


            if (TestVersions[version] == "Not Installed")
            {

           

                foreach (RadioButton rb in VersionsPanel.Children)
                {
                    rb.IsEnabled = true;
                }
                long time = 0;


                DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
                timer.Tick += (object s, EventArgs _e) =>
                {
                    string acc = "";
                    if (progressType == "download")
                        acc = $"- {progressBytesReceived / 1048576} of {progressBytesTotal / 1048576}MB";
                    else if (progressType == "Extracting")
                        acc = $"- Extracting {progressBytesReceived} of {progressBytesTotal}";
                    else if (progressType == "Installing")
                        acc = "Installing.... Please wait!";
                    else if (progressType == "backup")
                        acc = "Making a backup, Please wait! Might take long if you got huge amount of data.";

                    string[] tag = radioButton.Tag as string[];

                    string[] tags2 =
                    {
                        tag[0], version,
                        $"{progressPercentage}% " + acc
                    };
                    if(progressPercentage > 100)
                    {
                        tags2 = new []
                        {
                            tag[0], version,
                            acc
                        };
                    }
                    radioButton.Content = 415 - (progressPercentage / 100 * 415);
                    radioButton.Tag = tags2;

                    time += 50;

                    if (Minecraft.isInstalled() && time > 7000 && progressPercentage == 100 && progressType == "Installing")
                    {
                        Trace.WriteLine("yes 1");
                        timer.Stop();
                        radioButton.Style = FindResource("test1") as Style;
                        radioButton.IsChecked = true;
                        TestVersions[version] = "Installed";
                        ChosenVersion = version;
                        versionLabel.Content = ChosenVersion;
                    }
                };

                timer.Start();

                radioButton.Style = FindResource("test3") as Style;

                bool succeeded = await Task.Run(() => VersionManagement.InstallMinecraft(version));
                if (!succeeded)
                {
                    radioButton.Style = FindResource("test2") as Style;
                    radioButton.IsChecked = false;
                    TestVersions[version] = "Not Installed";
                }
                else
                {

                    foreach (var version2 in TestVersions)
                    {
                        if (version2.Key != version)
                        {
                            TestVersions[version2.Key] = "Click to Install";
                        }
                    }
                    progressPercentage = 100;
                 
                    
                }
            }
        }*/

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
            if (!String.IsNullOrEmpty((string)values[0]))
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
        public static readonly System.Windows.DependencyProperty StateProperty;
        public static readonly System.Windows.DependencyProperty VersionProperty;
        public static readonly System.Windows.DependencyProperty VersionLinkProperty;
        public static readonly System.Windows.DependencyProperty InstallPercentageProperty;
        public static readonly System.Windows.DependencyProperty ImageURLProperty;

        static VersionItemProperties()
        {
            StateProperty = System.Windows.DependencyProperty.RegisterAttached(
                "State", typeof(int), typeof(VersionItemProperties));
            VersionProperty = System.Windows.DependencyProperty.RegisterAttached(
                "Version", typeof(string), typeof(VersionItemProperties));
            VersionLinkProperty = System.Windows.DependencyProperty.RegisterAttached(
                "VersionLink", typeof(string), typeof(VersionItemProperties));
            InstallPercentageProperty = System.Windows.DependencyProperty.RegisterAttached(
                "InstallPercentage", typeof(string), typeof(VersionItemProperties));
            ImageURLProperty = System.Windows.DependencyProperty.RegisterAttached(
                "ImageURL", typeof(BitmapImage), typeof(VersionItemProperties));
        }

        public static void SetState(System.Windows.UIElement element, int value)
        {
            element.SetValue(StateProperty, value);
        }

        public static int GetState(System.Windows.UIElement element)
        {
            return (int)element.GetValue(StateProperty);
        }

        public static void SetVersion(System.Windows.UIElement element, string value)
        {
            element.SetValue(VersionProperty, value);
        }

        public static void SetVersionLink(System.Windows.UIElement element, string value)
        {
            element.SetValue(VersionLinkProperty, value);
        }

        public static string GetVersion(System.Windows.UIElement element)
        {
            return (string)element.GetValue(VersionProperty);
        }

        public static string GetVersionLink(System.Windows.UIElement element)
        {
            return (string)element.GetValue(VersionLinkProperty);
        }

        public static void SetInstallPercentage(System.Windows.UIElement element, string value)
        {
            element.SetValue(InstallPercentageProperty, value);
        }

        public static string GetInstallPercentage(System.Windows.UIElement element)
        {
            return (string)element.GetValue(InstallPercentageProperty);
        }

        public static void SetImageURL(System.Windows.UIElement element, BitmapImage value)
        {
            element.SetValue(ImageURLProperty, value);
        }

        public static BitmapImage GetImageURL(System.Windows.UIElement element)
        {
            return (BitmapImage)element.GetValue(ImageURLProperty);
        }
    }
}

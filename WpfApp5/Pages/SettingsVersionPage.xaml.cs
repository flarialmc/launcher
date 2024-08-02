using Flarial.Launcher.Managers;
using Flarial.Launcher.Styles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace Flarial.Launcher.Pages
{
    /// <summary>
    /// Interaction logic for SettingsVersionPage.xaml
    /// </summary>
    public partial class SettingsVersionPage : Page
    {
        public static StackPanel sp;
        public SettingsVersionPage()
        {
            InitializeComponent();
            sp = VersionItemStackPanel;

            string fileContent = new WebClient().DownloadString("https://raw.githubusercontent.com/flarialmc/newcdn/main/launcher/versions.json");

            Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(fileContent);

            foreach (var version in myDeserializedClass.Versions)
            {
                VersionItem versionItem = new VersionItem();
                VersionItemStackPanel.Children.Add(versionItem);
                VersionItemProperties.SetVersion(versionItem, version.Name);
                VersionItemProperties.SetVersionLink(versionItem, version.verlink);
                VersionItemProperties.SetState(versionItem, 0);

                foreach (string file in Directory.GetFiles(VersionManagement.launcherPath + "\\Versions"))
                {
                    if (file.Contains(version.Name))
                    {
                        VersionItemProperties.SetState(versionItem, 2);
                    }
                }
                if (Minecraft.GetVersion().ToString() == version.Name)
                {
                    VersionItemProperties.SetState(versionItem, 3);
                    versionItem.IsChecked = true;
                }

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(version.ImgURL); ;
                bitmapImage.EndInit();

                VersionItemProperties.SetImageURL(versionItem, bitmapImage);
            }

            VersionItem tempvi = (VersionItem)VersionItemStackPanel.Children[^1];
            tempvi.Margin = new Thickness(0);

            //if (Directory.GetFiles(VersionManagement.launcherPath + "\\Versions").Contains(VersionManagement.launcherPath + "\\Versions" + $"\\{version}"))

            //foreach (string file in Directory.GetFiles(VersionManagement.launcherPath + "\\Versions"))
            //{
            //    file.Contains()
            //}
        }
    }

    public class Root
    {
        public List<Version> Versions { get; set; }
    }

    public class Version
    {
        public string Name { get; set; }
        public string ImgURL { get; set; }
        
        public string verlink { get; set; }
    }
}

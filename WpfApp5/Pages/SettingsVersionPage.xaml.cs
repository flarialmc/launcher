using Flarial.Launcher.Managers;
using Flarial.Launcher.Styles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Flarial.Launcher.SDK;

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

        Task.Run(async () =>
        {
            var catalog = await Catalog.GetAsync();

            Dispatcher.Invoke(async () =>
            {

                string[] dir = Directory.GetFiles(VersionManagement.launcherPath + "\\Versions");
                foreach (var name in catalog.Reverse())
                {
                    Uri uri = await catalog.UriAsync(name);
                    VersionItem versionItem = new VersionItem();
                    VersionItemStackPanel.Children.Add(versionItem);
                    VersionItemProperties.SetVersion(versionItem, name);
                    VersionItemProperties.SetVersionLink(versionItem, uri.ToString());
                    VersionItemProperties.SetState(versionItem, 0);

                    bool unpackaged;
                    if (SDK.Minecraft.Installed) unpackaged = SDK.Minecraft.Unpackaged;
                    else unpackaged = false;
                    
                    if(unpackaged)
                    {
                        foreach (string file in dir)
                        {
                            if (file.Contains(name))
                            {
                                VersionItemProperties.SetState(versionItem, 2);
                            }
                        }
                    }

                    if (Minecraft.GetVersion().ToString() == name)
                    {
                        VersionItemProperties.SetState(versionItem, 3);
                        versionItem.IsChecked = true;
                    }
                    
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri("https://github.com/megahendick/Flarial.Laucher.Testing/blob/main/versionbg1.png?raw=true");
                    bitmapImage.EndInit();

                    VersionItemProperties.SetImageURL(versionItem, bitmapImage);
                    
                }
                

                if (VersionItemStackPanel.Children.Count > 0)
                {
                    Trace.WriteLine("Heya");
                    VersionItem tempvi = (VersionItem)VersionItemStackPanel.Children[VersionItemStackPanel.Children.Count - 1];
                    tempvi.Margin = new Thickness(0);
                }
            });
        });
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

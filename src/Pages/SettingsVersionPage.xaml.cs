using Flarial.Launcher.Managers;
using Flarial.Launcher.Styles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Flarial.Launcher.Pages;

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
        var window = (MainWindow)Application.Current.MainWindow;

        window.HomePage.IsEnabledChanged += (_, _) =>
        {
            string[] dir = Directory.GetFiles(VersionManagement.launcherPath + "\\Versions");
            foreach (var name in MainWindow.VersionCatalog.Reverse())
            {
                VersionItem versionItem = new();
                VersionItemStackPanel.Children.Add(versionItem);
                VersionItemProperties.SetVersion(versionItem, name);
                VersionItemProperties.SetState(versionItem, 0);

                bool unpackaged;
                if (SDK.Minecraft.Installed) unpackaged = SDK.Minecraft.Unpackaged;
                else unpackaged = false;

                if (unpackaged)
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
            }


            if (VersionItemStackPanel.Children.Count > 0)
            {
                Trace.WriteLine("Heya");
                VersionItem tempvi = (VersionItem)VersionItemStackPanel.Children[VersionItemStackPanel.Children.Count - 1];
                tempvi.Margin = new Thickness(0);
            }
        };
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

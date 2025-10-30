using Flarial.Launcher.Managers;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Styles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Flarial.Launcher.Pages;

/// <summary>
/// Interaction logic for SettingsVersionPage.xaml
/// </summary>
public partial class SettingsVersionPage : Page
{
    public static StackPanel sp;

    async void LaunchButtonIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs args)
    {
        await System.Windows.Threading.Dispatcher.Yield();

        string[] files = await Task.Run(() => Directory.GetFiles(VersionManagement.launcherPath + "\\Versions"));

        foreach (var name in MainWindow.VersionCatalog.Reverse())
        {
            VersionItem versionItem = new();
            VersionItemStackPanel.Children.Add(versionItem);
            VersionItemProperties.SetVersion(versionItem, name);
            VersionItemProperties.SetState(versionItem, 0);

            bool unpackaged = Minecraft.IsInstalled && Minecraft.IsUnpackaged;

            if (unpackaged)
            {
                foreach (string file in files)
                {
                    if (file.Contains(name)) VersionItemProperties.SetState(versionItem, 2);
                    await System.Windows.Threading.Dispatcher.Yield();
                }
            }

            if (MinecraftGame.GetVersion().ToString() == name)
            {
                VersionItemProperties.SetState(versionItem, 3);
                versionItem.IsChecked = true;
            }

            await System.Windows.Threading.Dispatcher.Yield();
        }

        if (VersionItemStackPanel.Children.Count > 0)
        {
            Trace.WriteLine("Heya");
            VersionItem tempvi = (VersionItem)VersionItemStackPanel.Children[VersionItemStackPanel.Children.Count - 1];
            tempvi.Margin = new Thickness(0);
        }

        var window = (MainWindow)Application.Current.MainWindow;
        window.LaunchButton.IsEnabledChanged -= LaunchButtonIsEnabledChanged;

        var settingsPage = (SettingsPage)window.SettingsFrame.Content;
        settingsPage.VersionsPageButton.IsEnabled = true;
    }

    public SettingsVersionPage()
    {
        InitializeComponent();
        sp = VersionItemStackPanel;

        var window = (MainWindow)Application.Current.MainWindow;
        window.LaunchButton.IsEnabledChanged += LaunchButtonIsEnabledChanged;
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

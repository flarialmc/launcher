using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Flarial.Launcher.Managers;
using Flarial.Launcher.Styles;

namespace Flarial.Launcher.Pages;

public partial class SettingsBackupPage : Page
{
    private static VirtualizingStackPanel _stackPanel;

    public SettingsBackupPage()
    {
        InitializeComponent();

        _stackPanel = BackupStackPanel;
    }

    public static void AddBackupItem(string time, string path) => _stackPanel.Children.Add(new BackupItem { Time = time, Path = path });

    //just placeholder stuff
    private void SettingsBackupPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.InvokeAsync(async () =>
        {
            List<string> backups = await BackupManager.GetAllBackupsAsync();

            foreach (string backup in backups)
            {
                AddBackupItem(backup, "what?");
            }
        });
    }
}
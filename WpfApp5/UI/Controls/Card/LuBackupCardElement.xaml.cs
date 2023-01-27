using Flarial.Launcher.Managers;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Flarial.Launcher.UI.Controls;

public partial class LuBackupCardElement : UserControl
{
    public event Action<object, RoutedEventArgs>? LoadButtonClicked;
    public event Action<object, RoutedEventArgs>? DeleteButtonClicked;

    public LuBackupCardElement()
    {
        InitializeComponent();
    }

    private async void LoadButton_OnClick(object sender, RoutedEventArgs e)
    {
        await BackupManager.loadBackup(BackupCard.Header.ToString());
    }

    private async void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        await BackupManager.DeleteBackup(BackupCard.Header.ToString());
        MainGrid.Children.Remove(BackupCard);
    }
}
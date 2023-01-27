using Flarial.Launcher.Managers;
using Octokit;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Flarial.Launcher.UI.Controls;

public partial class LuImportedCardElement : UserControl
{

    public LuImportedCardElement()
    {
        InitializeComponent();
    }

    private async void LoadButton_OnClick(object sender, RoutedEventArgs e)
    {

        try
        {
            if (Header.Text.Contains("Archived"))
            {
                InstallVersion.IsEnabled = false;
                RemoveVersion.IsEnabled = false;
                await VersionManagement.InstallMinecraft(Header.Text.Substring(0, Header.Text.Length - 36) + "x86");

                InstallVersion.IsEnabled = true;
                RemoveVersion.IsEnabled = true;
            }
            else
            {
                InstallVersion.IsEnabled = false;
                RemoveVersion.IsEnabled = false;
                await VersionManagement.InstallMinecraft(Header.Text);

                InstallVersion.IsEnabled = true;
                RemoveVersion.IsEnabled = true;

            }

        }
        catch (RateLimitExceededException)
        {
            InstallVersion.IsEnabled = true;
            MessageBox.Show("Octokit Rate Limit was reached.");
        }

    }

    private void RemoveVersion_Click(object sender, RoutedEventArgs e)
    {
        string path = Environment.GetFolderPath(
            (Environment.SpecialFolder.LocalApplicationData)) +
          "\\Chrones\\App\\Flarial\\Launcher\\Versions\\" + $"Minecraft{Header.Text}.Appx";
        File.Delete(path);
        MainGrid.Children.Remove(VersionCard);

        var versionCard = new LuVersionCardElement();
        versionCard.Header.Text = Header.Text;

        MainGrid.Children.Add(versionCard);

    }

}
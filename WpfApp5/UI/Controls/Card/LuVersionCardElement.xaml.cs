using Flarial.Launcher.Managers;
using Octokit;
using System.Windows;
using System.Windows.Controls;

namespace Flarial.Launcher.UI.Controls;

public partial class LuVersionCardElement : UserControl
{

    public LuVersionCardElement()
    {
        InitializeComponent();
    }

    private async void LoadButton_OnClick(object sender, RoutedEventArgs e)
    {

        try
        {
            if (Header.Text.Contains("Archived Non-RenderDragon"))
            {
                InstallVersion.IsEnabled = false;
                await VersionManagement.InstallMinecraft(Header.Text.Substring(0, Header.Text.Length - 36) + "x86");
                InstallVersion.IsEnabled = true;
                var importedcardElement = new LuImportedCardElement();
                importedcardElement.Header.Text = Header.Text;
                MainGrid.Children.Add(importedcardElement);

                MainGrid.Children.Remove(VersionCard);

            }
            else
            {
                InstallVersion.IsEnabled = false;
                await VersionManagement.InstallMinecraft(Header.Text);
                InstallVersion.IsEnabled = true;

                var importedcardElement = new LuImportedCardElement();
                importedcardElement.Header.Text = Header.Text;
                MainGrid.Children.Add(importedcardElement);

                MainGrid.Children.Remove(VersionCard);

            }
        }
        catch (RateLimitExceededException)
        {
            InstallVersion.IsEnabled = true;
            MessageBox.Show("Octokit Rate Limit was reached.");
        }
    }

}
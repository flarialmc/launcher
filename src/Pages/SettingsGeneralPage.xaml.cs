using Flarial.Launcher.Functions;
using Flarial.Launcher.Managers;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Flarial.Launcher.Pages;

/// <summary>
/// Interaction logic for SettingsGeneralPage.xaml
/// </summary>
public partial class SettingsGeneralPage : Page
{
    public static ToggleButton SaveConfigButton;

    public SettingsGeneralPage()
    {
        InitializeComponent();
        SaveConfigButton = SaveButton;

        CustomDllToggle.Checked += (_, _) =>
        {
            if ((bool)BetaDllToggle.IsChecked)
            {
                BetaDllToggle.IsChecked = false;
                Config.BetaDll = false;
            }
        };

        BetaDllToggle.Checked += (_, _) =>
        {
            if ((bool)CustomDllToggle.IsChecked)
            {
                CustomDllToggle.IsChecked = false;
                Config.CustomDll = false;
            }
        };

        ((MainWindow)Application.Current.MainWindow).HomePage.IsEnabledChanged += (_, _) =>
        {
            if (Config.CustomDll && Config.BetaDll) Config.CustomDll = Config.BetaDll = false;

            CustomDllToggle.IsChecked = Config.CustomDll;
            CustomDllPathTextBox.Value = Config.CustomDllPath;
            BetaDllToggle.IsChecked = Config.BetaDll;
            AutoLoginToggle.IsChecked = Config.AutoLogin;
            FixMinecraftMinimizingToggle.IsChecked = Config.FixMinimizing;
            EnableDiscordRpcToggle.IsChecked = Config.Rpc;
            ShowWelcomeMessageToggle.IsChecked = Config.WelcomeMessage;
            ParallaxEffectToggle.IsChecked = Config.BackgroundParallaxEffect;
        };
    }

    private void OnCustomDllToggleChecked(object sender, RoutedEventArgs e)
        => Animations.ToggleButtonTransitions.CheckedAnimation(CustomDllInputGrid);

    private void OnCustomDllToggleUnchecked(object sender, RoutedEventArgs e)
        => Animations.ToggleButtonTransitions.UnCheckedAnimation(CustomDllInputGrid);

    private void OnBetaDllToggleClicked(object sender, RoutedEventArgs e)
    {
        Config.BetaDll = (bool)((ToggleButton)sender).IsChecked;
        SaveConfigButton.IsChecked = true;
    }

    private void OnAutoLoginToggleClicked(object sender, RoutedEventArgs e)
    {
        Config.AutoLogin = (bool)((ToggleButton)sender).IsChecked;
        SaveConfigButton.IsChecked = true;
    }

    private void OnCustomDllToggleClicked(object sender, RoutedEventArgs e)
    {
        Config.CustomDll = (bool)((ToggleButton)sender).IsChecked;
        SaveConfigButton.IsChecked = true;
    }

    private void OnFixMinecraftMinimizingToggleClicked(object sender, RoutedEventArgs e)
    {
        Config.FixMinimizing = (bool)((ToggleButton)sender).IsChecked;
        SaveConfigButton.IsChecked = true;
    }

    private void OnEnableDiscordRpcToggleClicked(object sender, RoutedEventArgs e)
    {
        Config.Rpc = (bool)((ToggleButton)sender).IsChecked;

        if (Config.Rpc)
        {
            RPCManager.InitializeRPC();
        }
        else
        {
            RPCManager.StopRPC();
        }

        SaveConfigButton.IsChecked = true;
    }

    private void OnShowWelcomeMessageToggleClicked(object sender, RoutedEventArgs e)
    {
        Config.WelcomeMessage = (bool)((ToggleButton)sender).IsChecked;
        SaveConfigButton.IsChecked = true;
    }

    private void OnParallaxEffectToggleClicked(object sender, RoutedEventArgs e)
    {
        Config.BackgroundParallaxEffect = (bool)((ToggleButton)sender).IsChecked;
        if (!Config.BackgroundParallaxEffect)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.CenterParrallaxImage();
        }
        SaveConfigButton.IsChecked = true;
    }

    private void OnOpenLauncherFolderClicked(object sender, RoutedEventArgs e)
    {
        string launcherFolderPath = Managers.VersionManagement.launcherPath;
        if (Directory.Exists(launcherFolderPath))
        {
            Process.Start("explorer.exe", launcherFolderPath);
        }
        else
        {
            MainWindow.CreateMessageBox("Launcher folder not found!");
        }
    }

    private void OnOpenClientFolderClicked(object sender, RoutedEventArgs e)
    {
        string clientFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Packages",
            "Microsoft.MinecraftUWP_8wekyb3d8bbwe",
            "RoamingState",
            "Flarial"
        );

        if (Directory.Exists(clientFolderPath))
        {
            Process.Start("explorer.exe", clientFolderPath);
        }
        else
        {
            MainWindow.CreateMessageBox("Client folder not found!");
        }
    }

    private async void OnSaveButtonClicked(object sender, RoutedEventArgs e)
    {
        await Task.Run(() => Config.SaveConfig());
        SaveConfigButton.IsChecked = false;
    }
}
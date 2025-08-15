using Flarial.Launcher.Functions;
using Flarial.Launcher.SDK;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Environment;

namespace Flarial.Launcher.Pages;

/// <summary>
/// Interaction logic for SettingsGeneralPage.xaml
/// </summary>
public partial class SettingsGeneralPage : Page
{
    public static ToggleButton saveButton;

    //    readonly TextBlock _launcherFolderButtonTextBlock, _clientFolderButtonTextBlock;

    // readonly Border _launcherFolderButtonBorder, _clientFolderButtonBorder;

    static readonly string _clientPath;

    static readonly ProcessStartInfo _startInfo;

    static SettingsGeneralPage()
    {
        var localAppDataPath = GetFolderPath(SpecialFolder.LocalApplicationData);
        const string packagePath = @"Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\RoamingState\Flarial";

        _clientPath = System.IO.Path.Combine(localAppDataPath, packagePath);
        _startInfo = new()
        {
            UseShellExecute = true,
            FileName = _clientPath
        };
    }

    public SettingsGeneralPage()
    {
        InitializeComponent();

        BrushConverter brushConverter = new();

        LauncherFolderButton.ApplyTemplate();

        var launcherFolderButtonTextBlock = (TextBlock)LauncherFolderButton.Template.FindName("LaunchText", LauncherFolderButton);
        launcherFolderButtonTextBlock.Margin = new();
        launcherFolderButtonTextBlock.Text = "📁 Launcher";

        var launcherFolderButtonBorder = (Border)LauncherFolderButton.Template.FindName("MainBorder", LauncherFolderButton);
        launcherFolderButtonBorder.Background = (Brush)brushConverter.ConvertFromString("#3F2A2D");

        var launcherFolderButtonIcon = (System.Windows.Shapes.Path)LauncherFolderButton.Template.FindName("LaunchIcon", LauncherFolderButton);
        launcherFolderButtonIcon.Data = null;

        ClientFolderButton.ApplyTemplate();

        var clientFolderButtonTextBlock = (TextBlock)ClientFolderButton.Template.FindName("LaunchText", ClientFolderButton);
        clientFolderButtonTextBlock.Margin = new();
        clientFolderButtonTextBlock.Text = "📁 Client";

        var clientFolderButtonIcon = (System.Windows.Shapes.Path)ClientFolderButton.Template.FindName("LaunchIcon", ClientFolderButton);
        clientFolderButtonIcon.Data = null;

        var clientFolderButtonBorder = (Border)ClientFolderButton.Template.FindName("MainBorder", ClientFolderButton);
        clientFolderButtonBorder.Background = (Brush)brushConverter.ConvertFromString("#3F2A2D");


        saveButton = SaveButton;

        tb1.Checked += (_, _) =>
        {
            if ((bool)tb2.IsChecked)
            {
                tb2.IsChecked = false;
                Config.UseBetaDLL = false;
            }
        };

        tb2.Checked += (_, _) =>
        {
            if ((bool)tb1.IsChecked)
            {
                tb1.IsChecked = false;
                Config.UseCustomDLL = false;
            }
        };

        var window = (MainWindow)Application.Current.MainWindow;

        LauncherFolderButton.Click += (_, _) =>
        {
            using (Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = CurrentDirectory
            })) { }
        };

        ClientFolderButton.Click += (_, _) =>
        {
            if (!Directory.Exists(_clientPath))
            {
                MainWindow.CreateMessageBox("Please launch the client at least once to generate its folder.");
                return;
            }

            using (Process.Start(_startInfo)) { }
        };

        window.LaunchButton.IsEnabledChanged += LaunchButtonIsEnabledChanged;
    }

    void LaunchButtonIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs args)
    {
        if (Config.UseCustomDLL && Config.UseBetaDLL)
            Config.UseCustomDLL = Config.UseBetaDLL = false;

        tb1.IsChecked = Config.UseCustomDLL;
        tb2.IsChecked = Config.UseBetaDLL;
        tb3.IsChecked = Config.AutoLogin;
        tb4.IsChecked = Config.MCMinimized;
        HardwareAcceleration.IsChecked = Config.HardwareAcceleration;
        DLLTextBox.Value = Config.CustomDLLPath;

        var window = (MainWindow)Application.Current.MainWindow;
        window.LaunchButton.IsEnabledChanged -= LaunchButtonIsEnabledChanged;
    }

    private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
    => Animations.ToggleButtonTransitions.CheckedAnimation(DllGrid);

    private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        => Animations.ToggleButtonTransitions.UnCheckedAnimation(DllGrid);

    void HardwareAcceleration_Click(object sender, RoutedEventArgs e)
    {
        if (!(Config.HardwareAcceleration = (bool)((ToggleButton)sender).IsChecked))
            MainWindow.CreateMessageBox("Only disable hardware acceleration if you are having graphical issues in the launcher.");
        SaveButton.IsChecked = true;
    }

    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        Config.UseBetaDLL = (bool)((ToggleButton)sender).IsChecked;
        SaveButton.IsChecked = true;
    }

    private void ToggleButton_Click_1(object sender, RoutedEventArgs e)
    {
        Config.AutoLogin = (bool)((ToggleButton)sender).IsChecked;
        SaveButton.IsChecked = true;
    }

    private void ToggleButton_Click_2(object sender, RoutedEventArgs e)
    {
        Config.UseCustomDLL = (bool)((ToggleButton)sender).IsChecked;
        SaveButton.IsChecked = true;
    }

    private void ToggleButton_Click_3(object sender, RoutedEventArgs e)
    {
        Config.MCMinimized = (bool)((ToggleButton)sender).IsChecked;
        SaveButton.IsChecked = true;
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        await Task.Run(() => Config.SaveConfig());
        SaveButton.IsChecked = false;
    }
}
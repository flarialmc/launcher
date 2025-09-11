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
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Flarial.Launcher.Structures;
using static System.Environment;

namespace Flarial.Launcher.Pages;

/// <summary>
/// Interaction logic for SettingsGeneralPage.xaml
/// </summary>
public partial class SettingsGeneralPage : Page
{
    // ReSharper disable once MemberCanBePrivate.Global
    public double DllSelectionItemWidth { get; } = 120;
    // ReSharper disable once MemberCanBePrivate.Global
    public double DllSelectionItemMargin { get; } = 10;

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

        DataContext = this;
        
        // Start - Overrides some stuff from the Launch Buttom template.

        BrushConverter brushConverter = new();
        var brush = (Brush)brushConverter.ConvertFromString("#3F2A2D");

        LauncherFolderButton.ApplyTemplate();

        var launcherFolderButtonTextBlock = (TextBlock)LauncherFolderButton.Template.FindName("LaunchText", LauncherFolderButton);
        launcherFolderButtonTextBlock.Margin = new();
        launcherFolderButtonTextBlock.Text = "📁 Launcher";

        var launcherFolderButtonBorder = (Border)LauncherFolderButton.Template.FindName("MainBorder", LauncherFolderButton);
        launcherFolderButtonBorder.Background = brush;

        var launcherFolderButtonIcon = (System.Windows.Shapes.Path)LauncherFolderButton.Template.FindName("LaunchIcon", LauncherFolderButton);
        launcherFolderButtonIcon.Data = null;

        ClientFolderButton.ApplyTemplate();

        var clientFolderButtonTextBlock = (TextBlock)ClientFolderButton.Template.FindName("LaunchText", ClientFolderButton);
        clientFolderButtonTextBlock.Margin = new();
        clientFolderButtonTextBlock.Text = "📁 Client";

        var clientFolderButtonIcon = (System.Windows.Shapes.Path)ClientFolderButton.Template.FindName("LaunchIcon", ClientFolderButton);
        clientFolderButtonIcon.Data = null;

        var clientFolderButtonBorder = (Border)ClientFolderButton.Template.FindName("MainBorder", ClientFolderButton);
        clientFolderButtonBorder.Background = brush;

        // End - Overrides some stuff from the Launch Button template.

        saveButton = SaveButton;

        /*tb1.Checked += (_, _) =>
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
        };*/

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

        window.ContentRendered += Window_ContentRendered;
    }

    void AutoInject_Click(object sender, EventArgs args)
    {
        var button = (ToggleButton)sender;
        Config.AutoInject = (bool)button.IsChecked;

        if (Config.AutoInject)
            MainWindow.CreateMessageBox("The launcher will now auto-inject the client whenever Minecraft is launched.");
    }

    void MinimizeToTray_Click(object sender, EventArgs args)
    {
        var button = (ToggleButton)sender;
        Config.MinimizeToTray = (bool)button.IsChecked;

        if (!Config.MinimizeToTray)
        {
            Config.StartMinimized = false;
            StartMinimized.IsChecked = false;
        }
        else MainWindow.CreateMessageBox("The launcher will now minimize to tray.");
    }

    void StartMinimized_Click(object sender, EventArgs args)
    {
        var button = (ToggleButton)sender;
        Config.StartMinimized = (bool)button.IsChecked;

        if (Config.StartMinimized)
        {
            Config.MinimizeToTray = true;
            MinimizeToTray.IsChecked = true;
            MainWindow.CreateMessageBox("The launcher will now start minimized.");
        }
    }

    void Window_ContentRendered(object sender, EventArgs args)
    {
        /*if (Config.UseCustomDLL && Config.UseDLLBuild != 0)
        {
            Config.UseCustomDLL = false;
            Config.UseDLLBuild = 0;
        }*/

        if (Config.StartMinimized && !Config.MinimizeToTray)
            Config.StartMinimized = false;

        switch (Config.DllSelected)
        {
            case DllSelection.Stable:
                StableRadioButton.IsChecked = true;
                break;
            case DllSelection.Beta:
                BetaRadioButton.IsChecked = true;
                break;
            case DllSelection.Nightly:
                NightlyRadioButton.IsChecked = true;
                break;
            case DllSelection.Custom:
                CustomRadioButton.IsChecked = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        tb3.IsChecked = Config.AutoLogin;
        tb4.IsChecked = Config.MCMinimized;
        StartMinimized.IsChecked = Config.StartMinimized;
        HardwareAcceleration.IsChecked = Config.HardwareAcceleration;
        AutoInject.IsChecked = Config.AutoInject;
        MinimizeToTray.IsChecked = Config.MinimizeToTray;
        DLLTextBox.Value = Config.CustomDLLPath;

        var window = (MainWindow)Application.Current.MainWindow;
        if (window != null) window.ContentRendered -= Window_ContentRendered;
    }
    
    void HardwareAcceleration_Click(object sender, RoutedEventArgs e)
    {
        var isChecked = ((ToggleButton)sender).IsChecked;
        if (isChecked != null && !(Config.HardwareAcceleration = (bool)isChecked))
            MainWindow.CreateMessageBox("Only disable hardware acceleration if you are having graphical issues in the launcher.");
        SaveButton.IsChecked = true;
    }

    private void ToggleButton_Click_1(object sender, RoutedEventArgs e)
    {
        Config.AutoLogin = (bool)((ToggleButton)sender).IsChecked;
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

    private void BuildChanged(object sender, RoutedEventArgs e)
    {
        int.TryParse((sender as RadioButton)?.Tag.ToString(), out var num);
        var animation = new ThicknessAnimation
        {
            Duration = TimeSpan.FromMilliseconds(250),
            To = new Thickness(num * DllSelectionItemWidth + num * DllSelectionItemMargin, 0, 0, 0),
            EasingFunction = new QuadraticEase{ EasingMode = EasingMode.EaseInOut}
        };

        BuildSelectedBorder.BeginAnimation(MarginProperty, animation);

        if (Enum.TryParse((sender as RadioButton)?.Content.ToString(), out DllSelection dllSelection)) Config.DllSelected = dllSelection;

        if (dllSelection == DllSelection.Custom) Animations.ToggleButtonTransitions.CheckedAnimation(DllGrid);
    }
    
    private void CustomRadioButton_OnUnchecked(object sender, RoutedEventArgs e)
        => Animations.ToggleButtonTransitions.UnCheckedAnimation(DllGrid);
}
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
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

    readonly Settings _settings = Settings.Current;

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
        launcherFolderButtonTextBlock.Text = "Launcher";

        var launcherFolderButtonBorder = (Border)LauncherFolderButton.Template.FindName("MainBorder", LauncherFolderButton);
        launcherFolderButtonBorder.Background = brush;

        var launcherFolderButtonIcon = (System.Windows.Shapes.Path)LauncherFolderButton.Template.FindName("LaunchIcon", LauncherFolderButton);
        launcherFolderButtonIcon.Data = null;

        ClientFolderButton.ApplyTemplate();

        var clientFolderButtonTextBlock = (TextBlock)ClientFolderButton.Template.FindName("LaunchText", ClientFolderButton);
        clientFolderButtonTextBlock.Margin = new();
        clientFolderButtonTextBlock.Text = "Client";

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
                MainWindow.CreateMessageBox("⚠️ Please launch the client at least once to generate its folder.");
                return;
            }

            using (Process.Start(_startInfo)) { }
        };

        window.ContentRendered += Window_ContentRendered;
    }

    void Window_ContentRendered(object sender, EventArgs args)
    {
        switch (_settings.DllBuild)
        {
            case DllBuild.Release:
                ReleaseRadioButton.IsChecked = true;
                break;

            case DllBuild.Beta:
                if (BetaRadioButton.IsEnabled) BetaRadioButton.IsChecked = true;
                else ReleaseRadioButton.IsChecked = true;
                break;

            case DllBuild.Nightly:
                if (NightlyRadioButton.IsEnabled) NightlyRadioButton.IsChecked = true;
                else ReleaseRadioButton.IsChecked = true;
                break;

            case DllBuild.Custom:
                CustomRadioButton.IsChecked = true;
                break;
        }

        AutoLogin.IsChecked = _settings.AutoLogin;
        HardwareAcceleration.IsChecked = _settings.HardwareAcceleration;

        WaitForInitialization.IsChecked = _settings.WaitForInitialization;
        DLLTextBox.Value = _settings.CustomDllPath;

        var window = (MainWindow)Application.Current.MainWindow;
        if (window != null) window.ContentRendered -= Window_ContentRendered;
    }

    void WaitForInitializationClick(object sender, RoutedEventArgs args)
    {
        var button = (ToggleButton)sender;
        if (button.IsChecked is not bool @checked) return;
        _settings.WaitForInitialization = @checked;
    }

    void HardwareAcceleration_Click(object sender, RoutedEventArgs e)
    {
        var settings = Settings.Current;
        var button = (ToggleButton)sender;

        if (button.IsChecked is not bool @checked)
            return;

        settings.HardwareAcceleration = @checked;
    }

    private void ToggleButton_Click_1(object sender, RoutedEventArgs e)
    {
        var settings = Settings.Current;
        var button = (ToggleButton)sender;

        if (button.IsChecked is not bool @checked)
            return;

        settings.AutoLogin = @checked;
    }

    private void BuildChanged(object sender, RoutedEventArgs e)
    {
        int.TryParse((sender as RadioButton)?.Tag.ToString(), out var num);
        var animation = new ThicknessAnimation
        {
            Duration = TimeSpan.FromMilliseconds(250),
            To = new Thickness(num * DllSelectionItemWidth + num * DllSelectionItemMargin, 0, 0, 0),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        BuildSelectedBorder.BeginAnimation(MarginProperty, animation);

        var settings = Settings.Current;
        var button = (RadioButton)sender;
        var content = $"{button.Content}";

        if (Enum.TryParse<DllBuild>(content, out var build))
            settings.DllBuild = build;

        if (build is DllBuild.Custom)
            Animations.ToggleButtonTransitions.CheckedAnimation(DllGrid);
    }

    private void CustomRadioButton_OnUnchecked(object sender, RoutedEventArgs e)
        => Animations.ToggleButtonTransitions.UnCheckedAnimation(DllGrid);
}
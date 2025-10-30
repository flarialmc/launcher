using Flarial.Launcher.Functions;
using Flarial.Launcher.Managers;
using Flarial.Launcher.Pages;
using Flarial.Launcher.Animations;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Interop;
using Windows.ApplicationModel;
using static System.StringComparison;
using Flarial.Launcher.Services.Client;
using Flarial.Launcher.Services.Modding;
using Flarial.Launcher.Services.Management;
using Flarial.Launcher.Services.Networking;
using Flarial.Launcher.Services.Core;

namespace Flarial.Launcher;

public partial class MainWindow
{
    public int version = 200;

    public static int progressPercentage;

    public static long progressBytesReceived, progressBytesTotal;

    public static string progressType;

    public static bool isPremium, isLoggedIn, Reverse, isDownloadingVersion;

    public static ImageBrush PFP;

    public static TextBlock StatusLabel, versionLabel, Username;

    private static StackPanel mbGrid;

    private static readonly Stopwatch speed = new();

    internal readonly WindowInteropHelper WindowInteropHelper;

    public static SDK.Catalog VersionCatalog;

    public bool IsLaunchEnabled
    {
        get { return (bool)GetValue(IsLaunchEnabledProperty); }
        set { SetValue(IsLaunchEnabledProperty, value); }
    }

    public static readonly DependencyProperty IsLaunchEnabledProperty =
        DependencyProperty.Register("IsLaunchEnabled", typeof(bool),
            typeof(MainWindow), new PropertyMetadata(true));

    public bool updateTextEnabled
    {
        get { return (bool)GetValue(updateTextEnabledProperty); }
        set { SetValue(updateTextEnabledProperty, value); }
    }

    public static readonly DependencyProperty updateTextEnabledProperty =
        DependencyProperty.Register("updateTextEnabled", typeof(bool),
            typeof(MainWindow), new PropertyMetadata(true));

    public int updateProgress
    {
        get { return (int)GetValue(updateProgressProperty); }
        set { SetValue(updateProgressProperty, value); }
    }

    public static readonly DependencyProperty updateProgressProperty =
        DependencyProperty.Register("updateProgress", typeof(int),
            typeof(MainWindow), new PropertyMetadata(0));

    readonly TextBlock _launchButtonTextBlock;

    readonly Settings _settings = Settings.Current;

    public MainWindow()
    {
        InitializeComponent();

        LaunchButton.ApplyTemplate();
        _launchButtonTextBlock = (TextBlock)LaunchButton.Template.FindName("LaunchText", LaunchButton);
        _launchButtonTextBlock.Text = "Updating...";

        WindowInteropHelper = new(this);

        Icon = EmbeddedResources.GetImageSource("app.ico");
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        SnapsToDevicePixels = UseLayoutRounding = true;

        MouseLeftButtonDown += (_, _) => { try { DragMove(); } catch { } };
        ContentRendered += MainWindow_ContentRendered;

        Stopwatch stopwatch = new();
        speed.Start();
        stopwatch.Start();

        Trace.WriteLine("Debug 0 " + stopwatch.Elapsed.Milliseconds.ToString());

        Trace.WriteLine("Debug 0.5 " + stopwatch.Elapsed.Milliseconds.ToString());

        Trace.WriteLine("Debug 1 " + stopwatch.Elapsed.Milliseconds.ToString());

        LauncherVersion.Text = "v" + Assembly.GetExecutingAssembly().GetName().Version;

        Trace.WriteLine("Debug 2 " + stopwatch.Elapsed.Milliseconds.ToString());

        Dispatcher.InvokeAsync(MinecraftGame.Init);
        Trace.WriteLine("Debug 3 " + stopwatch.Elapsed.Milliseconds.ToString());

        StatusLabel = statusLabel;
        versionLabel = VersionLabel;
        Username = username;
        mbGrid = MbGrid;
        PFP = pfp;
        SettingsPage.MainGrid = MainGrid;
        SettingsPage.b1 = MainBorder;

        Dispatcher.InvokeAsync(RPCManager.Initialize);

        Trace.WriteLine("Debug 9 " + stopwatch.Elapsed.Milliseconds.ToString());
        Application.Current.MainWindow = this;

        SetGreetingLabel();
        Trace.WriteLine("Debug 10 " + stopwatch.Elapsed.Milliseconds.ToString());

        stopwatch.Stop();

        IsLaunchEnabled = false;

        StartRefreshTimer();
    }

    private System.Timers.Timer refreshTimer;

    private void StartRefreshTimer()
    {
        refreshTimer = new System.Timers.Timer(1.5 * 60 * 1000);
        refreshTimer.Elapsed += (sender, e) => RefreshWebView();
        refreshTimer.AutoReset = true;
        refreshTimer.Enabled = true;
    }

    private void RefreshWebView()
    {
    }

    readonly PackageCatalog Catalog = PackageCatalog.OpenForCurrentUser();

    async Task UpdateGameVersionAsync(Package package)
    {
        if (package.Id.FamilyName.Equals("Microsoft.MinecraftUWP_8wekyb3d8bbwe", OrdinalIgnoreCase))
            await UpdateGameVersionAsync();
    }

    async Task UpdateGameVersionAsync() => await Task.Run(async () =>
    {
        try
        {
            var text = Minecraft.UsingGameDevelopmentKit ? Minecraft.PackageVersion : Minecraft.ClientVersion;
            var compatible = await VersionCatalog.CompatibleAsync();

            Dispatcher.Invoke(() =>
            {
                VersionLabel.Text = text;
                VersionTextBorder.Background = compatible ? _darkGreen : _darkRed;
            });
        }
        catch
        {
            Dispatcher.Invoke(() =>
            {
                VersionLabel.Text = "0.0.0";
                VersionTextBorder.Background = _darkGoldenrod;
            });
        }
    });

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        _ = Task.WhenAll(CheckLicenseAsync(), SetCampaignBannerAsync());
        CreateMessageBox("📢 Join our Discord! https://flarial.xyz/discord");

        if (!_settings.HardwareAcceleration)
            CreateMessageBox("⚠️ Hardware acceleration is disabled.");
    }

    async Task SetCampaignBannerAsync()
    {
        try
        {
            var imageSource = await Sponsors.GetLiteByteCampaignBannerAsync();
            if (imageSource is not null) AdBorder.Background = new ImageBrush()
            {
                ImageSource = imageSource,
                Stretch = Stretch.UniformToFill
            };
        }
        catch { }
    }

    static readonly SolidColorBrush _darkRed = new(Colors.DarkRed);

    static readonly SolidColorBrush _darkGreen = new(Colors.DarkGreen);

    static readonly SolidColorBrush _darkGoldenrod = new(Colors.DarkGoldenrod);

    async Task CheckLicenseAsync()
    {
        try { LicenseText.Foreground = await LicensingService.VerifyAsync() ? _darkGreen : _darkRed; }
        catch { LicenseText.Foreground = _darkGoldenrod; }
    }

    private async void MainWindow_ContentRendered(object sender, EventArgs e)
    {
        if (await LauncherUpdater.CheckAsync())
        {
            updateTextEnabled = true;

            Dispatcher.Invoke(() =>
            {
                MainGrid.IsEnabled = false;
                MainGrid.Visibility = Visibility.Hidden;
                mbGrid.Visibility = Visibility.Hidden;
                LolGrid.Visibility = Visibility.Visible;
                LolGrid.IsEnabled = true;
            });

            await LauncherUpdater.DownloadAsync(LauncherDownloadProgressAction);
            return;
        }

        _launchButtonTextBlock.Text = "Preparing...";
        VersionCatalog = await SDK.Catalog.GetAsync();

        Catalog.PackageInstalling += async (_, args) => { if (args.IsComplete) await UpdateGameVersionAsync(args.Package); };
        Catalog.PackageUninstalling += async (_, args) => { if (args.IsComplete) await UpdateGameVersionAsync(args.Package); };
        Catalog.PackageUpdating += async (_, args) => { if (args.IsComplete) await UpdateGameVersionAsync(args.TargetPackage); };

        _ = UpdateGameVersionAsync();
        _launchButtonTextBlock.Text = "Launch";
        IsLaunchEnabled = true;
    }

    public static void CreateMessageBox(string text)
    {
        mbGrid.Children.Add(new Styles.MessageBox { Text = text });
    }

    private void MoveWindow(object sender, MouseButtonEventArgs e) => DragMove();

    private void WindowMinimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void WindowClose(object sender, RoutedEventArgs e) => Close();

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e) =>
        SettingsPageTransition.SettingsEnterAnimation(MainBorder, MainGrid);

    private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) =>
        NewsPageTransition.Animation(Reverse, MainBorder, NewsBorder, NewsArrow);

    private void SetGreetingLabel()
    {
        int Time = int.Parse(DateTime.Now.ToString("HH", System.Globalization.DateTimeFormatInfo.InvariantInfo));

        if (Time >= 0 && Time < 12)
            GreetingLabel.Text = "Good Morning!";
        else if (Time >= 12 && Time < 18)
            GreetingLabel.Text = "Good Afternoon!";
        else if (Time >= 18 && Time <= 24)
            GreetingLabel.Text = "Good Evening!";
    }

    private async void Inject_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var build = _settings.DllBuild;
            var path = _settings.CustomDllPath;
            var custom = build is DllBuild.Custom;
            var initialized = _settings.WaitForInitialization;
            var beta = build is DllBuild.Beta or DllBuild.Nightly;
            var client = beta ? FlarialClient.Beta : FlarialClient.Release;

            IsLaunchEnabled = false;
            _launchButtonTextBlock.Text = "Verifying...";

            if (!Minecraft.IsInstalled)
            {
                CreateMessageBox(@"⚠️ Please install the game.");
                return;
            }

            Minecraft.HasUWPAppLifecycle = true;
            var gdk = Minecraft.UsingGameDevelopmentKit;

            if (custom)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    CreateMessageBox("⚠️ Please specify a Custom DLL.");
                    return;
                }

                ModificationLibrary library = new(path);

                if (!library.IsValid)
                {
                    CreateMessageBox("⚠️ The specified Custom DLL is potentially invalid or doesn't exist.");
                    return;
                }

                _launchButtonTextBlock.Text = "Launching...";
                await Task.Run(() => (gdk ? Injector.GDK : Injector.UWP).Launch(initialized, library));

                StatusLabel.Text = "Launched Custom DLL.";
                return;
            }

            if (gdk)
            {
                CreateMessageBox("⚠️ GDK builds aren’t supported yet — join our Discord to stay updated!");
                return;
            }

            bool compatible = beta || await VersionCatalog.CompatibleAsync(); if (!compatible)
            {
                CreateMessageBox("⚠️ This version not supported by the client.");
                return;
            }

            await client.DownloadAsync(ClientDownloadProgressAction);

            _launchButtonTextBlock.Text = "Launching...";
            await Task.Run(() => client.LaunchGame(initialized));

            StatusLabel.Text = $"Launched {(beta ? "Beta" : "Release")} DLL.";
        }
        finally
        {
            _launchButtonTextBlock.Text = "Launch";
            IsLaunchEnabled = true;
        }
    }


    public void ClientDownloadProgressAction(int value) => Dispatcher.Invoke(() =>
    {
        _launchButtonTextBlock.Text = "Downloading...";
        statusLabel.Text = $"Downloading... {value}%";
    });

    public void LauncherDownloadProgressAction(int value)
    {
        Dispatcher.Invoke(() =>
        {
            updateProgress = value;
        });
    }

    private void Window_OnClosing(object sender, CancelEventArgs e)
    {
        if (isDownloadingVersion)
        {
            e.Cancel = true;
            CreateMessageBox("⚠️ The launcher cannot be closed due a version of Minecraft being downloaded.");
        }
    }

    protected override void OnClosed(EventArgs args)
    {
        base.OnClosed(args);
        Settings.Current.Save();
        Environment.Exit(0);
    }

    static readonly ProcessStartInfo _startInfo = new()
    {
        UseShellExecute = true,
        FileName = Sponsors.LiteByteCampaignUri
    };

    private void AdBorder_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        using (Process.Start(_startInfo)) { }
    }
}

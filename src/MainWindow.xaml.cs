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
using Flarial.Launcher.Services.SDK;
using Flarial.Launcher.Services.Management;
using Flarial.Launcher.Services.Game;
using Flarial.Launcher.Styles;
using Flarial.Launcher.Services.Networking;

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

    public static DialogBox MainWindowDialogBox;

    private static StackPanel mbGrid;

    private static readonly Stopwatch speed = new();

    internal readonly WindowInteropHelper WindowInteropHelper;

    public static Catalog VersionCatalog;

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
RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);

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
        MainWindowDialogBox = DialogControl;
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

    readonly PackageCatalog PackageCatalog = PackageCatalog.OpenForCurrentUser();

    void UpdateGameVersionText(Package package) { if (package.Id.FamilyName.Equals("Microsoft.MinecraftUWP_8wekyb3d8bbwe", OrdinalIgnoreCase)) UpdateGameVersionText(); }

    void UpdateGameVersionText() => Dispatcher.BeginInvoke(() =>
    {
        try
        {
            VersionLabel.Text = $"{(Minecraft.UsingGameDevelopmentKit ? "GDK" : "UWP")} ~ {Minecraft.Version}";
            VersionTextBorder.Background = VersionCatalog.IsCompatible ? _darkGreen : _darkRed;
        }
        catch
        {
            VersionLabel.Text = "? ~ 0.0.0";
            VersionTextBorder.Background = _darkGoldenrod;
        }
    });

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        _ = Task.WhenAll(CheckLicenseAsync(), SetCampaignBannerAsync());
        CreateMessageBox("📢 Join our Discord! https://flarial.xyz/discord");

        if (!_settings.HardwareAcceleration) CreateMessageBox("⚠️ Hardware acceleration is disabled.");
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
        if (!await FlarialClient.CanConnectAsync() && await DialogBox.ShowAsync("🚨 Connection Failure", @"Failed to connect to Flarial's CDN.
        
• Try restarting the launcher.
• Check your internet connection.
• Change your system DNS for both IPv4 and IPv6.

If you need help, join our Discord.", ("Exit", true), ("Continue", false))) { Close(); return; }

        if (await FlarialLauncher.CheckAsync() && await DialogBox.ShowAsync("💡 Launcher Update", @"An update is available for the launcher.

• Updating the launcher provides new bug fixes & features.
• Newer versions of the client & game might require a launcher update.

If you need help, join our Discord.", ("Update", true)))
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

            await FlarialLauncher.DownloadAsync(LauncherDownloadProgressAction);
            return;
        }

        _launchButtonTextBlock.Text = "Preparing...";
        VersionCatalog = await Catalog.GetAsync();

        PackageCatalog.PackageInstalling += (_, args) => { if (args.IsComplete) UpdateGameVersionText(args.Package); };
        PackageCatalog.PackageUninstalling += (_, args) => { if (args.IsComplete) UpdateGameVersionText(args.Package); };
        PackageCatalog.PackageUpdating += (_, args) => { if (args.IsComplete) UpdateGameVersionText(args.TargetPackage); };

        UpdateGameVersionText();
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
            IsLaunchEnabled = false;
            _launchButtonTextBlock.Text = "Verifying...";

            if (!Minecraft.Installed)
            {
                CreateMessageBox(@"⚠️ Please install the game.");
                return;
            }

            var build = _settings.DllBuild;
            var path = _settings.CustomDllPath;
            var custom = build is DllBuild.Custom;
            var initialized = _settings.WaitForInitialization;
            var beta = build is DllBuild.Beta or DllBuild.Nightly || Minecraft.UsingGameDevelopmentKit;
            var client = beta ? FlarialClient.Beta : FlarialClient.Release;

            if (custom)
            {
                if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
                {
                    CreateMessageBox("⚠️ Please specify a Custom DLL.");
                    return;
                }

                Library library = new(path); if (!library.IsLoadable)
                {
                    CreateMessageBox("⚠️ The specified Custom DLL is potentially invalid or doesn't exist.");
                    return;
                }

                _launchButtonTextBlock.Text = "Launching...";
                if (await Task.Run(() => Injector.Launch(initialized, library)) is not { })
                    CreateMessageBox("💡 Please close the game & try again.");

                StatusLabel.Text = "Launched Custom DLL.";
                return;
            }

            if (!beta && !VersionCatalog.IsCompatible)
            {
                await DialogBox.ShowAsync("⚠️ Unsupported Version", @"The currently installed version is unsupported by the client.

• Try switching to a version supported by the client.
• Try using the beta build of client via [Settings] -> [General].

If you need help, join our Discord.", ("OK", true));
                return;
            }

            if (beta && !await DialogBox.ShowAsync("⚠️ Beta Usage", @"The beta build of the client might be potentially unstable. 

• Bugs & crashes might occur frequently during gameplay.
• The beta build is meant for reporting bugs & issues with the client.

Hence use at your own risk.", ("Cancel", false), ("Launch", true)))
                return;

            if (!await client.DownloadAsync(ClientDownloadProgressAction))
            {
                await DialogBox.ShowAsync("💡 Update Failed", @"A client update couldn't be downloaded.

• Try closing the game & see if the client updates.
• Try rebooting your machine & see if that resolves the issue.

If you need help, join our Discord.", ("OK", true));
                return;
            }

            _launchButtonTextBlock.Text = "Launching...";
            if (!await Task.Run(() => client.Launch(initialized)))
                await DialogBox.ShowAsync("💡 Launch Failure", @"The client couldn't inject correctly.

• Try closing the game & try again.
• Remove & disable any 3rd party mods or tools.

If you need help, join our Discord.", ("OK", true));

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

    void LauncherDownloadProgressAction(int value) => Dispatcher.Invoke(() => { updateProgress = value; });

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

using Flarial.Launcher.Functions;
using Flarial.Launcher.Managers;
using Flarial.Launcher.Pages;
using Flarial.Launcher.Animations;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Interop;
using Bedrockix.Windows;
using Bedrockix.Minecraft;
using Windows.ApplicationModel;
using System.Windows.Forms;
using Flarial.Launcher.SDK;

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

    readonly NotifyIcon _notifyIcon = new() { Text = "Flarial Launcher", Visible = false };

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

        _notifyIcon.Icon = EmbeddedResources.GetIcon("app.ico");
        _notifyIcon.Click += NotifyIcon_Click;

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

        Dispatcher.InvokeAsync(Minecraft.Init);
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
        System.Windows.Application.Current.MainWindow = this;

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

    async Task UpdateVersionLabel(Package source, bool value)
    {
        if (source.Id.FamilyName.Equals("Microsoft.MinecraftUWP_8wekyb3d8bbwe", StringComparison.OrdinalIgnoreCase))
            await Task.Run(() =>
            {
                var text = value ? SDK.Minecraft.Version : "0.0.0";
                Dispatcher.Invoke(() => VersionLabel.Text = text);
            });
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        if (Game.Installed) VersionLabel.Text = SDK.Minecraft.Version;

        Catalog.PackageInstalling += async (_, args) => { if (args.IsComplete) await UpdateVersionLabel(args.Package, true); };
        Catalog.PackageUpdating += async (_, args) => { if (args.IsComplete) await UpdateVersionLabel(args.TargetPackage, true); };
        Catalog.PackageUninstalling += async (_, args) => { if (args.IsComplete) await UpdateVersionLabel(args.Package, false); };

        Task.WhenAll(CheckLicenseAsync(), SetCampaignBannerAsync());
        CreateMessageBox("📢 Join our Discord! https://flarial.xyz/discord");
        if (!_settings.HardwareAcceleration) CreateMessageBox("⚠️ Hardware acceleration is disabled.");
    }

    async Task SetCampaignBannerAsync()
    {
        try
        {
            var imageSource = await Sponsors.GetLiteByteCampaignBanner();
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

    static readonly SolidColorBrush _darkYellow = new(Colors.DarkGoldenrod);

    async Task CheckLicenseAsync()
    {
        try
        {
            var @checked = await Licensing.CheckAsync();
            VersionTextBorder.Background = @checked ? _darkGreen : _darkRed;
            if (!@checked) CreateMessageBox("❌ Please purchase a genuine copy of the game.");
        }
        catch
        {
            VersionTextBorder.Background = _darkYellow;
            CreateMessageBox("⚠️ Couldn't verify game ownership.");
        }
    }

    private async void MainWindow_ContentRendered(object sender, EventArgs e)
    {
        if (await SDK.Launcher.AvailableAsync())
        {
            updateTextEnabled = true;

            Dispatcher.Invoke(() =>
            {
                MainGrid.IsEnabled = false;
                MainGrid.Visibility = Visibility.Hidden;
                mbGrid.Visibility = Visibility.Hidden;
                LolGrid.Visibility = Visibility.Visible;
                LolGrid.IsEnabled = true;
            }, DispatcherPriority.ApplicationIdle);

            await SDK.Launcher.UpdateAsync(DownloadProgressCallback2);
        }

        _launchButtonTextBlock.Text = "Preparing...";
        VersionCatalog = await SDK.Catalog.GetAsync();

        _launchButtonTextBlock.Text = "Launch";
        IsLaunchEnabled = true;

        if (_settings.StartMinimized)
            WindowMinimize(null, null);

        GameEvents.Launched += GameEventsLaunched;
    }

    void GameEventsLaunched() => Dispatcher.BeginInvoke(async () =>
    {
        if (!_settings.AutoInject) return;
        if (!Game.Installed) return;
        if (SDK.Minecraft.GDK) return;
        if (!IsLaunchEnabled) return;

        IsLaunchEnabled = false;
        _launchButtonTextBlock.Text = "Launching...";

        var result = await Task.Run(() => Game.Launch());
        if (result is not null) Inject_Click(null, null);
        else
        {
            IsLaunchEnabled = true;
            _launchButtonTextBlock.Text = "Launch";
        }
    });

    public static void CreateMessageBox(string text)
    {
        mbGrid.Children.Add(new Styles.MessageBox { Text = text });
    }

    void NotifyIcon_Click(object sender, EventArgs args)
    {
        _notifyIcon.Visible = false;
        Visibility = Visibility.Visible;
    }

    private void MoveWindow(object sender, MouseButtonEventArgs e) => DragMove();

    private void WindowMinimize(object sender, RoutedEventArgs e)
    {
        if (_settings.MinimizeToTray)
        {
            _notifyIcon.Visible = true;
            Visibility = Visibility.Hidden;
        }
        else WindowState = WindowState.Minimized;
    }

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
            var custom = build is Settings.DllSelection.Custom;
            var beta = build is Settings.DllSelection.Beta or Settings.DllSelection.Nightly;

            IsLaunchEnabled = false;
            _launchButtonTextBlock.Text = "Verifying...";

            if (!SDK.Minecraft.Installed)
            {
                CreateMessageBox("⚠️ Please install Minecraft from the Microsoft Store or Xbox App.");
                return;
            }

            if (SDK.Minecraft.GDK)
            {
                CreateMessageBox("⚠️ Flarial Client doesn't support GDK builds of Minecraft currently.");
                return;
            }

            if (await Task.Run(() => Metadata.Instancing))
            {
                CreateMessageBox("⚠️ Multi-instancing is not supported, please disable it.");
                return;
            }

            if (custom)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    CreateMessageBox("⚠️ Please specify a Custom DLL.");
                    return;
                }

                Library library = new(path);

                if (!library.Valid)
                {
                    CreateMessageBox("⚠️ The specified Custom DLL is potentially invalid or doesn't exist.");
                    return;
                }

                await Task.Run(() => Loader.Launch(library));
                StatusLabel.Text = "Launched Custom DLL! Enjoy!";

                return;
            }

            bool compatible = await VersionCatalog.CompatibleAsync() || beta;

            if (!compatible)
            {
                CreateMessageBox("⚠️ This version not supported by the client.");
                return;
            }

            await Client.DownloadAsync(beta, DownloadProgressCallback);

            _launchButtonTextBlock.Text = "Launching...";
            await Client.LaunchAsync(beta);

            StatusLabel.Text = $"Launched {(beta ? "Beta" : "Stable")} DLL! Enjoy.";
        }
        finally
        {
            _launchButtonTextBlock.Text = "Launch";
            IsLaunchEnabled = true;
        }
    }


    public void DownloadProgressCallback(int value) => Dispatcher.Invoke(() =>
    {
        _launchButtonTextBlock.Text = "Downloading...";
        statusLabel.Text = $"Downloading... {value}%";
    });

    public void DownloadProgressCallback2(int value)
    {
        Dispatcher.Invoke(() =>
        {
            updateProgress = value;
        });
    }

    [Obsolete("The launcher saves settings automatically when closed.")]
    private async void SaveConfig(object sender, RoutedEventArgs e) => await Task.Run(Settings.Save);

    private void Window_OnClosing(object sender, CancelEventArgs e)
    {
        if (isDownloadingVersion)
        {
            e.Cancel = true;
            CreateMessageBox("The launcher cannot be closed due a version of Minecraft being downloaded.");
        }
    }

    protected override void OnClosed(EventArgs args)
    {
        base.OnClosed(args);
        _notifyIcon.Dispose();

        Trace.Close();
        Settings.Save();
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

[Obsolete("Broken & buggy, look for alternatives or deprecate entirely.", true)]
public class AutoFlushTextWriterTraceListener(FileStream stream) : TextWriterTraceListener(stream)
{
    public override async void Write(string message)
    {
        await Writer.WriteAsync(message).ConfigureAwait(false);
        Writer.Flush();
    }

    public override async void WriteLine(string message)
    {
        await Writer.WriteLineAsync(message).ConfigureAwait(false);
        Writer.Flush();
    }
}

[Obsolete("Broken & buggy, look for alternatives or deprecate entirely.", true)]
public class FileTraceListener : TraceListener
{
    private readonly StreamWriter _writer;

    public FileTraceListener(string filePath)
    {
        _writer = new StreamWriter(filePath, true)
        {
            AutoFlush = true // Enable AutoFlush if needed
        };
    }

    public override async void Write(string message)
    {
        await _writer.WriteAsync(message).ConfigureAwait(false);
    }

    public override async void WriteLine(string message)
    {
        await _writer.WriteLineAsync(message).ConfigureAwait(false);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _writer?.Flush();
            _writer?.Close();
            _writer?.Dispose();
        }
        base.Dispose(disposing);
    }
}


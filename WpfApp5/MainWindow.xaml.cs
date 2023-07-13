﻿using Flarial.Launcher.Functions;
using Flarial.Launcher.Managers;
using Flarial.Launcher.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Octokit;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using RadioButton = System.Windows.Controls.RadioButton;

namespace Flarial.Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public int duration = 300;

        Storyboard myWidthAnimatedButtonStoryboard1 = new Storyboard();
        Storyboard myWidthAnimatedButtonStoryboard2 = new Storyboard();
        Storyboard myWidthAnimatedButtonStoryboard3 = new Storyboard();
        public Window1 w = new();
        public bool ifBeta;
        public double version = 0.666; // 0.666 will be ignored by the updater, hence it wont update. But for release, it is recommended to use an actual release number.
        public string minecraft_version = "amongus";
        public string custom_dll_path = "amongus";
        public string custom_theme_path = "main_default";
        public bool closeToTray;
        public bool isLoggedIn;
        private Dictionary<string, string> TestVersions = new Dictionary<string, string>();
        private string ChosenVersion;
        // PLZ REMBER TO CHECK IF USER IS BETA TOO. DONT GO AROUND USING THIS OR ELS PEOPL CAN HAC BETTA DLL!!
        public bool shouldUseBetaDLL;
        private ImageSource guestImage;
        public static int progressPercentage;
        public static long progressBytesReceived;
        public static long progressBytesTotal;

        public MainWindow()
        {
            loadConfig();

            if (!Utils.IsAdministrator)
            {
                MessageBox.Show("Run the application as an Administrator to continue.");
                Process.GetCurrentProcess().Kill();
            }

            Minecraft.Init();
            CreateDirectoriesAndFiles();

            Environment.CurrentDirectory = VersionManagement.launcherPath;

            InitializeComponent();

            if (custom_dll_path != "amongus")
                CustomDllButton.IsChecked = true;

            TrayButton.IsChecked = closeToTray;

            if (!(custom_theme_path == "main_default"))
            {
                if (!string.IsNullOrEmpty(custom_theme_path))
                {
                    var app = (App)Application.Current;
                    app.ChangeTheme(new Uri(custom_theme_path, UriKind.Absolute));
                }
            }

            BetaDLLButton.IsChecked = shouldUseBetaDLL;

            InitializeAnimations();

            OptionsButton.Click += OptionsButton_Click;
            RadioButton3.Checked += RadioButton3_Checked;
            LoginGuest.Click += LoginGuest_Click;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "powershell.exe";
            startInfo.Arguments = "set-executionpolicy unrestricted";
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            WebClient webClient = new WebClient();
            webClient.DownloadFile(new Uri("https://cdn.flarial.net/updater.ps1"), "updater.ps1");
            string latestVer = webClient.DownloadString(new Uri("https://cdn.flarial.net/launcher/latestVersion.txt"));
            Trace.WriteLine(latestVer);
            double among = Convert.ToDouble(latestVer);
            if (version != 0.666 && version <= among)
            {
                startInfo.Arguments = "updater.ps1";
                process.StartInfo = startInfo;
                process.Start();
                Environment.Exit(0);
            }
            else if (version == 0.666)
            {
                Trace.WriteLine("It's development time.");
            }

            TestVersions = new Dictionary<string, string>
    {
        { "1.16.100.4", "Not Installed" },
        { "1.19.51.1", "Selected" },
        { "1.19.70", "Installed" }
    };

            foreach (string version in TestVersions.Keys)
            {
                AddRadioButton(version, TestVersions[version]);
            }

            versionLabel.Content = Minecraft.GetVersion();
            SetGreetingLabel();

            Task.Delay(1);

            Dispatcher.BeginInvoke(() => RPCManager.Initialize());

            Application.Current.MainWindow = this;
        }

        private void CreateDirectoriesAndFiles()
        {
            if (!Directory.Exists(BackupManager.backupDirectory))
                Directory.CreateDirectory(BackupManager.backupDirectory);

            if (!Directory.Exists(VersionManagement.launcherPath + "Versions\\"))
                Directory.CreateDirectory(VersionManagement.launcherPath + "Versions\\");

            if (!Directory.Exists(VersionManagement.launcherPath))
                Directory.CreateDirectory(VersionManagement.launcherPath);

            if (!File.Exists($"{VersionManagement.launcherPath}\\cachedToken.txt"))
                File.Create($"{VersionManagement.launcherPath}\\cachedToken.txt");
        }

        private void InitializeAnimations()
        {

            DoubleAnimation myDoubleAnimation1 = CreateDoubleAnimation(130, 600, duration);
            DoubleAnimation myDoubleAnimation2 = CreateDoubleAnimation(600, 130, duration);
            DoubleAnimation myDoubleAnimation3 = CreateDoubleAnimation(50, 400, duration);
            DoubleAnimation myDoubleAnimation4 = CreateDoubleAnimation(400, 50, duration);
            DoubleAnimation myDoubleAnimation5 = CreateDoubleAnimation(0, 1, duration);
            DoubleAnimation myDoubleAnimation6 = CreateDoubleAnimation(1, 0, duration);
            DoubleAnimation myDoubleAnimation7 = CreateDoubleAnimation(600, 130, duration);
            DoubleAnimation myDoubleAnimation8 = CreateDoubleAnimation(400, 50, duration);
            DoubleAnimation myDoubleAnimation9 = CreateDoubleAnimation(1, 0, duration);
            ThicknessAnimation myPointAnimation1 = CreateThicknessAnimation(new Thickness(145, 140, 0, 0), new Thickness(0, 0, 0, 0), duration);
            ThicknessAnimation myPointAnimation2 = CreateThicknessAnimation(new Thickness(0, 0, 0, 0), new Thickness(145, 140, 0, 0), duration);
            ThicknessAnimation myPointAnimation3 = CreateThicknessAnimation(new Thickness(0, 0, 0, 0), new Thickness(15, 0, 0, 15), duration);

            Storyboard.SetTargetName(myDoubleAnimation1, OptionsGrid.Name);
            Storyboard.SetTargetName(myDoubleAnimation2, OptionsGrid.Name);
            Storyboard.SetTargetName(myDoubleAnimation3, OptionsGrid.Name);
            Storyboard.SetTargetName(myDoubleAnimation4, OptionsGrid.Name);
            Storyboard.SetTargetName(myDoubleAnimation5, OptionsGrid.Name);
            Storyboard.SetTargetName(myDoubleAnimation6, OptionsGrid.Name);
            Storyboard.SetTargetName(myDoubleAnimation7, LoginGrid.Name);
            Storyboard.SetTargetName(myDoubleAnimation8, LoginGrid.Name);
            Storyboard.SetTargetName(myDoubleAnimation9, LoginGrid.Name);
            Storyboard.SetTargetName(myPointAnimation1, OptionsGrid.Name);
            Storyboard.SetTargetName(myPointAnimation2, OptionsGrid.Name);
            Storyboard.SetTargetName(myPointAnimation3, LoginGrid.Name);
            Storyboard.SetTargetProperty(myDoubleAnimation1, new PropertyPath(Grid.WidthProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation2, new PropertyPath(Grid.WidthProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation7, new PropertyPath(Grid.WidthProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation3, new PropertyPath(Grid.HeightProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation4, new PropertyPath(Grid.HeightProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation8, new PropertyPath(Grid.HeightProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation5, new PropertyPath(Grid.OpacityProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation6, new PropertyPath(Grid.OpacityProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation9, new PropertyPath(Grid.OpacityProperty));
            Storyboard.SetTargetProperty(myPointAnimation1, new PropertyPath(Grid.MarginProperty));
            Storyboard.SetTargetProperty(myPointAnimation2, new PropertyPath(Grid.MarginProperty));
            Storyboard.SetTargetProperty(myPointAnimation3, new PropertyPath(Grid.MarginProperty));

            myWidthAnimatedButtonStoryboard1.Children.Add(myDoubleAnimation1);
            myWidthAnimatedButtonStoryboard2.Children.Add(myDoubleAnimation2);
            myWidthAnimatedButtonStoryboard1.Children.Add(myDoubleAnimation3);
            myWidthAnimatedButtonStoryboard2.Children.Add(myDoubleAnimation4);
            myWidthAnimatedButtonStoryboard1.Children.Add(myDoubleAnimation5);
            myWidthAnimatedButtonStoryboard2.Children.Add(myDoubleAnimation6);
            myWidthAnimatedButtonStoryboard3.Children.Add(myDoubleAnimation7);
            myWidthAnimatedButtonStoryboard3.Children.Add(myDoubleAnimation8);
            myWidthAnimatedButtonStoryboard3.Children.Add(myDoubleAnimation9);
            myWidthAnimatedButtonStoryboard1.Children.Add(myPointAnimation1);
            myWidthAnimatedButtonStoryboard2.Children.Add(myPointAnimation2);
            myWidthAnimatedButtonStoryboard3.Children.Add(myPointAnimation3);

            /*OptionsButton.Click += async delegate (object sender, RoutedEventArgs args)
            {
                RadioButton0.IsChecked = true;
                OptionsGrid.Visibility = Visibility.Visible;
                //myWidthAnimatedButtonStoryboard1.Begin(OptionsGrid);
                await Task.Delay(duration);
                MainGrid.Visibility = Visibility.Hidden;
            };

            RadioButton3.Checked += async delegate (object sender, RoutedEventArgs args)
            {
                MainGrid.Visibility = Visibility.Visible;
                //myWidthAnimatedButtonStoryboard2.Begin(OptionsGrid);
                await Task.Delay(duration);
                OptionsGrid.Visibility = Visibility.Hidden;
            };

            LoginGuest.Click += async delegate (object sender, RoutedEventArgs args)
            {
                MainGrid.Visibility = Visibility.Visible;
                //myWidthAnimatedButtonStoryboard3.Begin(LoginGrid);
                await Task.Delay(duration);
                LoginGrid.Visibility = Visibility.Hidden;
            };*/
        }

        private DoubleAnimation CreateDoubleAnimation(double from, double to, int duration)
        {
            return new DoubleAnimation
            {
                From = from,
                To = to,
                EasingFunction = new QuadraticEase(),
                Duration = new Duration(TimeSpan.FromMilliseconds(duration))
            };
        }

        private ThicknessAnimation CreateThicknessAnimation(Thickness from, Thickness to, int duration)
        {
            return new ThicknessAnimation
            {
                From = from,
                To = to,
                EasingFunction = new QuadraticEase(),
                Duration = new Duration(TimeSpan.FromMilliseconds(duration))
            };
        }

        private async void OptionsButton_Click(object sender, RoutedEventArgs args)
        {
            RadioButton0.IsChecked = true;
            OptionsGrid.Visibility = Visibility.Visible;
            myWidthAnimatedButtonStoryboard1.Begin(OptionsGrid);
            await Task.Delay(duration);
            MainGrid.Visibility = Visibility.Hidden;
        }

        private async void RadioButton3_Checked(object sender, RoutedEventArgs args)
        {
            MainGrid.Visibility = Visibility.Visible;
            myWidthAnimatedButtonStoryboard2.Begin(OptionsGrid);
            await Task.Delay(duration);
            OptionsGrid.Visibility = Visibility.Hidden;
        }

        private async void LoginGuest_Click(object sender, RoutedEventArgs args)
        {
            MainGrid.Visibility = Visibility.Visible;
            //myWidthAnimatedButtonStoryboard3.Begin(LoginGrid);
            //await Task.Delay(duration);
            LoginGrid.Visibility = Visibility.Hidden;
        }

        private void AddRadioButton(string version, string status)
        {
            RadioButton radioButton = new RadioButton();
            Style? style1 = null;
            string[] tags = { "/Images/Gus1.png", version, "temp" };

            if (status == "Installed")
                style1 = this.FindResource("test1") as Style;
            else if (status == "Selected")
            {
                style1 = this.FindResource("test1") as Style;
                radioButton.IsChecked = true;
            }
            else if (status == "Not Installed")
                style1 = this.FindResource("test2") as Style;

            radioButton.Style = style1;
            radioButton.Tag = tags;
            radioButton.Checked += RadioButton_Checked;

            VersionsPanel.Children.Add(radioButton);
        }

        private async void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            object[] tags = (object[])radioButton.Tag;
            string version = tags[1].ToString();
            

            if (TestVersions[version] == "Not Installed")
            {
                radioButton.Style = FindResource("test3") as Style;

                foreach (RadioButton rb in VersionsPanel.Children)
                {
                    rb.IsEnabled = true;
                }

                DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
                timer.Tick += Timer_Tick;
                timer.Start();

                void Timer_Tick(object sender, EventArgs e)
                {
                        string[] tags2 =
                        {
                            "/Images/Gus1.png", version,
                            $"{progressPercentage}% - {progressBytesReceived / 1048576} of {progressBytesTotal / 1048576}MB"
                        };
                        radioButton.Content = 415 - (progressPercentage / 100 * 415);
                        radioButton.Tag = tags2;

                        if (progressPercentage == 100)
                        {
                            timer.Stop();
                            radioButton.Style = FindResource("test1") as Style;
                            radioButton.IsChecked = true;
                            TestVersions[version] = "Installed";
                        }
                }
            }

            ChosenVersion = version;
            versionLabel.Content = ChosenVersion;

            try
            {
                await Task.Run(() => VersionManagement.InstallMinecraft(ChosenVersion));
            }
            catch (RateLimitExceededException)
            {
                MessageBox.Show("Octokit Rate Limit was reached.");
            }
        }

        private void SetGreetingLabel()
        {
            int Time = Int32.Parse(DateTime.Now.ToString("HH", System.Globalization.DateTimeFormatInfo.InvariantInfo));

            if (Time >= 0 && Time < 12)
                GreetingLabel.Content = "Good Morning!";
            else if (Time >= 12 && Time < 18)
                GreetingLabel.Content = "Good Afternoon!";
            else if (Time >= 18 && Time <= 24)
                GreetingLabel.Content = "Good Evening!";
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            if (e.Delta < 0)
            {
                scrollViewer.LineRight();
            }
            else
            {
                scrollViewer.LineLeft();
            }
            e.Handled = true;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e) => this.DragMove();
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if (closeToTray == false) Environment.Exit(0);
            this.Hide();
        }

        private void HideGrid(object sender, RoutedEventArgs e)
        {
            MainGrid.Visibility = Visibility.Visible;
            LoginGrid.Visibility = Visibility.Hidden;
            this.Hide();
        }

        private void loadConfig()
        {
            ConfigData? config = Config.getConfig();

            if (config == null)
            {
                return;
            }

            minecraft_version = config.minecraft_version;
            shouldUseBetaDLL = config.shouldUseBetaDll;
            custom_dll_path = config.custom_dll_path;
            closeToTray = config.closeToTray;
            custom_theme_path = config.custom_theme_path;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var result = await AttemptLogin();
            if (result) { return; }

            w.Show();

            await w.web.EnsureCoreWebView2Async();
            w.web.CoreWebView2.Navigate("https://discord.com/api/oauth2/authorize?client_id=1067854754518151168&redirect_uri=https%3A%2F%2Fflarial.net&response_type=code&scope=guilds%20identify%20guilds.members.read");
            w.web.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
        }

        private async void CoreWebView2_NavigationStarting(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri.StartsWith("https://flarial.net"))
            {
                await AttemptLoginWithoutCache(e);
            }
        }

        private async Task<bool> AttemptLoginWithoutCache(Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            try
            {
                if (Uri.TryCreate(e.Uri, UriKind.Absolute, out Uri? uri) && uri != null)
                {
                    string code = uri.Query.TrimStart('?').Split('&').FirstOrDefault(p => p.StartsWith("code="))?.Substring(5);
                    if (!string.IsNullOrEmpty(code))
                    {
                        string rawToken = Auth.postReq(code);
                        var atd = JsonConvert.DeserializeObject<AccessTokenData>(rawToken);
                        await Auth.CacheToken(atd.access_token, DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(atd.expires_in));
                        string userResponse = Auth.getReqUser(atd.access_token);
                        LoginAccount(userResponse, atd.access_token);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        private async Task<bool> AttemptLogin()
        {
            var cached = await Auth.GetCache();
            if (cached != null && cached.expiry > DateTime.Now)
            {
                string userResponse = Auth.getReqUser(cached.access_token);
                LoginAccount(userResponse, cached.access_token);
                return true;
            }
            return false;
        }

        private async void LoginAccount(string userResponse, string authToken)
        {
            DiscordUser user = JsonConvert.DeserializeObject<DiscordUser>(userResponse);
            if (user != null)
            {
                Username.Content = user.username;
                Username2.Content = user.username;
                guestImage = PFP.Source;

                if (user.avatar != null)
                {
                    var imageSource = new BitmapImage(new Uri($"https://cdn.discordapp.com/avatars/{user.id}/{user.avatar}.png"));
                    PFP.Source = imageSource;
                    PFP2.Source = imageSource;
                }
            }

            string guildUserContent = Auth.getReqGuildUser(authToken);
            DiscordGuildUser guildUser = JsonConvert.DeserializeObject<DiscordGuildUser>(guildUserContent);
            if (guildUser.roles != null)
            {
                if (guildUser.roles.Contains("1050447423635460197") ||
                    guildUser.roles.Contains("1058465209443958816") ||
                    guildUser.roles.Contains("1059109828267606066") ||
                    guildUser.roles.Contains("1059332938774364160") ||
                    guildUser.roles.Contains("1059408198261551145"))
                {
                    ifBeta = true;
                    BetaDLLButton.Visibility = Visibility.Visible;
                    if (guildUser.roles.Contains("1058465209443958816"))
                        DevTag.Visibility = Visibility.Visible;
                    else if (guildUser.roles.Contains("1059109828267606066"))
                        StaffTag.Visibility = Visibility.Visible;
                    else if (guildUser.roles.Contains("1059332938774364160") && guildUser.roles.Contains("1059408198261551145"))
                        BetaDLLButton.Visibility = Visibility.Visible;
                    else if (guildUser.roles.Contains("1059332938774364160"))
                        StaffTag.Visibility = Visibility.Visible;
                    else if (guildUser.roles.Contains("1059408198261551145"))
                        BetaTag.Visibility = Visibility.Visible;
                    Trace.WriteLine("iz beta bro");
                }
                else
                {
                    Trace.WriteLine("No no no NOT BETA BRO!");
                }
            }

            w.Close();
            isLoggedIn = true;
            LoginButton.Visibility = Visibility.Hidden;
            LogoutButton.Visibility = Visibility.Visible;
            LoginGrid.Visibility = Visibility.Hidden;
            MainGrid.Visibility = Visibility.Visible;
        }




        private async void Inject_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(custom_dll_path))
            {
                WebClient webClient = new WebClient();
                DownloadProgressChangedEventHandler among = new DownloadProgressChangedEventHandler(DownloadProgressCallback);
                webClient.DownloadProgressChanged += among;
                await webClient.DownloadFileTaskAsync(new Uri("https://horion.download/dll"), "Among.dll");
                await Injector.Inject($"{Managers.VersionManagement.launcherPath}\\Among.dll", statusLabel);
            }
            else
            {
                if (File.Exists(custom_dll_path) && Path.GetExtension(custom_dll_path) == ".dll")
                {
                    await Injector.Inject(custom_dll_path, statusLabel);
                }
            }
        }


        public void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)

        {

            // Displays the operation identifier, and the transfer progress.

            statusLabel.Content = $" Downloaded {e.ProgressPercentage}% of client";
        }

        /*private void Options_Click(object sender, RoutedEventArgs e)
        {
            
            
            
            BetaDLLButton.IsChecked = shouldUseBetaDLL;
            if (custom_dll_path != "amongus")
            {
                CustomDllButton.IsChecked = true;
            }
            else
            {
                CustomDllButton.IsChecked = false;

            }
            if (closeToTray == true)
            {
                TrayButton.IsChecked = true;
            }
            else
            {
                TrayButton.IsChecked = false;

            }
            OptionsGrid.Visibility = Visibility.Visible;
            MainGrid.Visibility = Visibility.Hidden;
        }*/

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (closeToTray == false) Environment.Exit(0);
            else this.Hide();
        }


        //could have avoided a shitty solution like this but i don't care enough to implement MVVM
        private void OptionsBackClick(object sender, RoutedEventArgs e)
        {
            OptionsGrid.Visibility = Visibility.Hidden;
            MainGrid.Visibility = Visibility.Visible;
            RadioButton0.IsChecked = true;
            RadioButton3.IsChecked = false;
        }

        private void OptionsGeneralClick(object sender, RoutedEventArgs e)
        {
            OptionsVerionGrid.Visibility = Visibility.Hidden;
            OptionsGeneralGrid.Visibility = Visibility.Visible;
            OptionsAccountGrid.Visibility = Visibility.Hidden;
        }

        private void OptionsVersionClick(object sender, RoutedEventArgs e)
        {
            OptionsVerionGrid.Visibility = Visibility.Visible;
            OptionsGeneralGrid.Visibility = Visibility.Hidden;
            OptionsAccountGrid.Visibility = Visibility.Hidden;
        }

        private void OptionsAccountClick(object sender, RoutedEventArgs e)
        {
            OptionsVerionGrid.Visibility = Visibility.Hidden;
            OptionsGeneralGrid.Visibility = Visibility.Hidden;
            OptionsAccountGrid.Visibility = Visibility.Visible;
        }

        //i could implement the OpenFileDialog my self but im only responisble for the frontend + im lazy as shit
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (custom_dll_path == "amongus")
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.InitialDirectory = @"C:\";
                dialog.DefaultExt = "dll";
                dialog.Filter = "DLL Files|*.dll;";
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                dialog.Multiselect = false;
                dialog.Title = "Select Custom DLL";

                if (dialog.ShowDialog() == true)
                {

                    custom_dll_path = dialog.FileName;
                    Trace.WriteLine(custom_dll_path);

                }
                else
                {
                    dialog.ShowDialog();
                    custom_dll_path = dialog.FileName;
                    Trace.WriteLine(custom_dll_path);
                }
            }
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            custom_dll_path = "amongus";
            Trace.WriteLine(custom_dll_path);
        }

        private void BetaButton_Checked(object sender, RoutedEventArgs e)
        {
            shouldUseBetaDLL = true;
        }

        private void BetaButton_Unchecked(object sender, RoutedEventArgs e)
        {
            shouldUseBetaDLL = false;
        }

        private void TrayButton_Checked(object sender, RoutedEventArgs e)
        {
            closeToTray = true;
        }

        private void TrayButton_Unchecked(object sender, RoutedEventArgs e)
        {
            closeToTray = false;
        }

        //same here
        private void Logout(object sender, RoutedEventArgs e)
        {
            ifBeta = false;
            PFP.Source = guestImage;
            PFP2.Source = guestImage;
            Username.Content = "Guest";
            Username2.Content = "Guest";
            LoginButton.Visibility = Visibility.Visible;
            LogoutButton.Visibility = Visibility.Hidden;
            LoginGrid.Visibility = Visibility.Visible;
            MainGrid.Visibility = Visibility.Hidden;
            OptionsGrid.Visibility = Visibility.Hidden;
            BetaDLLButton.Visibility = Visibility.Collapsed;
            DevTag.Visibility = Visibility.Collapsed;
            ExecTag.Visibility = Visibility.Collapsed;
            StaffTag.Visibility = Visibility.Collapsed;
            ModTag.Visibility = Visibility.Collapsed;
            BetaTag.Visibility = Visibility.Collapsed;
            MediaTag.Visibility = Visibility.Collapsed;
            isLoggedIn = false;
        }

        private void LoginAmongus(object sender, RoutedEventArgs e)
        {
            LoginGrid.Visibility = Visibility.Visible;
            MainGrid.Visibility = Visibility.Hidden;
            OptionsGrid.Visibility = Visibility.Hidden;
            LoginGrid.Margin = new Thickness(0,0,0,0);
            LoginGrid.Width = 600;
            LoginGrid.Height = 400;
            LoginGrid.Opacity = 1;
        }

        private async void SaveConfig(object sender, RoutedEventArgs e)
        {
            await Config.saveConfig(minecraft_version, false, custom_dll_path, shouldUseBetaDLL, closeToTray);
            var app = (App)Application.Current;
            app.ChangeTheme(new Uri(custom_theme_path, UriKind.Absolute));
        }

        private void Window_OnClosing(object? sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CustomDialogBox MessageBox = new CustomDialogBox("Why do we need this?", "We need this to verify if you have Flarial Beta, the discord login page is in a webview so none of your login info is shared with us.", "MessageBox");
            MessageBox.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = @"C:\";
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Multiselect = false;
            dialog.Title = "Select Custom DLL";

            if (dialog.ShowDialog() == true)
            {

                custom_theme_path = dialog.FileName;
                Trace.WriteLine(custom_dll_path);

            }
            else
            {
                custom_theme_path = "main_default";
            }
        }

        private void CustomThemeButton_Unchecked(object sender, RoutedEventArgs e)
        {
            custom_theme_path = "main_default";
        }

        private void CustomThemeButton_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}

public class ShowMessageCommand : ICommand
{
    public void Execute(object parameter)
    {
        if (parameter.ToString() == "Among")
        {
            if (MessageBox.Show("Show application?", "Flarial", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Application.Current.MainWindow.Show();
            }
        }
    }

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;
}
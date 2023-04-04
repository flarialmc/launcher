using Flarial.Launcher.Functions;
using Flarial.Launcher.Managers;
using Flarial.Launcher.Structures;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Octokit;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using RadioButton = System.Windows.Controls.RadioButton;
using Button = System.Windows.Controls.Button;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace Flarial.Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        public Window1 w = new();
        public bool ifBeta;
        public double version = 0.666; // 0.666 will be ignored by the updater, hence it wont update. But for release, it is recommended to use an actual release number.
        public string minecraft_version = "amongus";
        public string custom_dll_path = "amongus";
        public bool closeToTray;
        public bool isLoggedIn;
        // PLZ REMBER TO CHECK IF USER IS BETA TOO. DONT GO AROUND USING THIS OR ELS PEOPL CAN HAC BETTA DLL!!
        public bool shouldUseBetaDLL;
        private ImageSource guestImage;

        public MainWindow()
        {
            
            
            loadConfig();

            if (!Utils.IsAdministrator)
            {
                MessageBox.Show("Run the application as an Administrator to continue.");
                Process.GetCurrentProcess().Kill();
            }
            Minecraft.Init();

            if (!Directory.Exists(BackupManager.backupDirectory)) { Directory.CreateDirectory(BackupManager.backupDirectory); }
            if (!Directory.Exists(VersionManagement.launcherPath + "Versions\\")) { Directory.CreateDirectory(VersionManagement.launcherPath + "Versions\\"); }
            if (!Directory.Exists(VersionManagement.launcherPath))
            {
                Directory.CreateDirectory(VersionManagement.launcherPath);
            }

            if (!File.Exists($"{VersionManagement.launcherPath}\\cachedToken.txt"))
                File.Create($"{VersionManagement.launcherPath}\\cachedToken.txt");



            Environment.CurrentDirectory = VersionManagement.launcherPath;

            InitializeComponent();
            
            if (custom_dll_path != "amongus")
                CustomDllButton.IsChecked = true;

            TrayButton.IsChecked = closeToTray;

            BetaDLLButton.IsChecked = shouldUseBetaDLL;

            int Duration = 300;

            DoubleAnimation myDoubleAnimation1 = new DoubleAnimation();
            myDoubleAnimation1.From = 130;
            myDoubleAnimation1.To = 600;
            myDoubleAnimation1.EasingFunction = new QuadraticEase();
            myDoubleAnimation1.Duration = new Duration(TimeSpan.FromMilliseconds(Duration));
            DoubleAnimation myDoubleAnimation2 = new DoubleAnimation();
            myDoubleAnimation2.From = 600;
            myDoubleAnimation2.To = 130;
            myDoubleAnimation2.EasingFunction = new QuadraticEase();
            myDoubleAnimation2.Duration = new Duration(TimeSpan.FromMilliseconds(Duration));
            DoubleAnimation myDoubleAnimation7 = new DoubleAnimation();
            myDoubleAnimation7.From = 600;
            myDoubleAnimation7.To = 130;
            myDoubleAnimation7.EasingFunction = new QuadraticEase();
            myDoubleAnimation7.Duration = new Duration(TimeSpan.FromMilliseconds(Duration));
            DoubleAnimation myDoubleAnimation3 = new DoubleAnimation();
            myDoubleAnimation3.From = 50;
            myDoubleAnimation3.To = 400;
            myDoubleAnimation3.EasingFunction = new QuadraticEase();
            myDoubleAnimation3.Duration = new Duration(TimeSpan.FromMilliseconds(Duration));
            DoubleAnimation myDoubleAnimation4 = new DoubleAnimation();
            myDoubleAnimation4.From = 400;
            myDoubleAnimation4.To = 50;
            myDoubleAnimation4.EasingFunction = new QuadraticEase();
            myDoubleAnimation4.Duration = new Duration(TimeSpan.FromMilliseconds(Duration));
            DoubleAnimation myDoubleAnimation8 = new DoubleAnimation();
            myDoubleAnimation8.From = 400;
            myDoubleAnimation8.To = 50;
            myDoubleAnimation8.EasingFunction = new QuadraticEase();
            myDoubleAnimation8.Duration = new Duration(TimeSpan.FromMilliseconds(Duration));
            DoubleAnimation myDoubleAnimation5 = new DoubleAnimation();
            myDoubleAnimation5.From = 0;
            myDoubleAnimation5.To = 1;
            myDoubleAnimation5.EasingFunction = new QuadraticEase();
            myDoubleAnimation5.Duration = new Duration(TimeSpan.FromMilliseconds(Duration));
            DoubleAnimation myDoubleAnimation6 = new DoubleAnimation();
            myDoubleAnimation6.From = 1;
            myDoubleAnimation6.To = 0;
            myDoubleAnimation6.EasingFunction = new QuadraticEase();
            myDoubleAnimation6.Duration = new Duration(TimeSpan.FromMilliseconds(Duration));
            DoubleAnimation myDoubleAnimation9 = new DoubleAnimation();
            myDoubleAnimation9.From = 1;
            myDoubleAnimation9.To = 0;
            myDoubleAnimation9.EasingFunction = new QuadraticEase();
            myDoubleAnimation9.Duration = new Duration(TimeSpan.FromMilliseconds(Duration));
            ThicknessAnimation myPointAnimation1 = new ThicknessAnimation();
            myPointAnimation1.From = new Thickness(145, 140, 0, 0);
            myPointAnimation1.To = new Thickness(0, 0, 0, 0);
            myPointAnimation1.EasingFunction = new QuadraticEase();
            myPointAnimation1.Duration = new Duration(TimeSpan.FromMilliseconds(Duration));
            ThicknessAnimation myPointAnimation2 = new ThicknessAnimation();
            myPointAnimation2.From = new Thickness(0, 0, 0, 0);
            myPointAnimation2.EasingFunction = new QuadraticEase();
            myPointAnimation2.To = new Thickness(145, 140, 0, 0);
            myPointAnimation2.Duration = new Duration(TimeSpan.FromMilliseconds(Duration));
            ThicknessAnimation myPointAnimation3 = new ThicknessAnimation();
            myPointAnimation3.From = new Thickness(0, 0, 0, 0);
            myPointAnimation3.EasingFunction = new QuadraticEase();
            myPointAnimation3.To = new Thickness(15, 0, 0, 15);
            myPointAnimation3.Duration = new Duration(TimeSpan.FromMilliseconds(Duration));
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
            Storyboard myWidthAnimatedButtonStoryboard1 = new Storyboard();
            Storyboard myWidthAnimatedButtonStoryboard2 = new Storyboard();
            Storyboard myWidthAnimatedButtonStoryboard3 = new Storyboard();
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

            OptionsButton.Click += async delegate (object sender, RoutedEventArgs args)
            {
                RadioButton0.IsChecked = true;
                OptionsGrid.Visibility = Visibility.Visible;
                myWidthAnimatedButtonStoryboard1.Begin(OptionsGrid);
                await Task.Delay(Duration);
                MainGrid.Visibility = Visibility.Hidden;
            };
            RadioButton3.Checked += async delegate (object sender, RoutedEventArgs args)
            {
                MainGrid.Visibility = Visibility.Visible;
                myWidthAnimatedButtonStoryboard2.Begin(OptionsGrid);
                await Task.Delay(Duration);
                OptionsGrid.Visibility = Visibility.Hidden;
            };
            LoginGuest.Click += async delegate (object sender, RoutedEventArgs args)
            {
                MainGrid.Visibility = Visibility.Visible;
                myWidthAnimatedButtonStoryboard3.Begin(LoginGrid);
                await Task.Delay(Duration);
                LoginGrid.Visibility = Visibility.Hidden;
            };

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
            } else if (version == 0.666)
            {
                Trace.WriteLine("It's development time.");
            }

            //this is just for testing and a placeholder so feel free to change it to best fit your needs, you'll probably figure it out
            string[] TestVersions = { "1.16.100.4", "1.19.51.1" };
            string ChosenVersion;

            foreach(string version in TestVersions)
            {
                ImageSource[] imagesources = new ImageSource[2] { new BitmapImage(new Uri($"/Images/{version}.jpg", UriKind.RelativeOrAbsolute)), new BitmapImage(new Uri($"/Images/{version}_2.jpg", UriKind.RelativeOrAbsolute))};
                Style style = this.FindResource("VersionRadioButton") as Style;
                RadioButton radioButton = new RadioButton();
                radioButton.Style = style;
                radioButton.Tag = imagesources;
                if (Minecraft.GetVersion().ToString().StartsWith(version.Remove(5))) radioButton.IsChecked = true;
                radioButton.Checked += RadioButton_Checked;

                VerisonPanel.Children.Add(radioButton);

                async void RadioButton_Checked(object sender, RoutedEventArgs e)
                {
                    ChosenVersion = version;
                    versionLabel.Content = ChosenVersion;
                    
                    try
                    {

                       await Task.Run(() => VersionManagement.InstallMinecraft(ChosenVersion));
                       
                    } catch(RateLimitExceededException)
                    {
                        MessageBox.Show("Octokit Rate Limit was reached.");
                    }
                }
                
                
            }
            

            versionLabel.Content = Minecraft.GetVersion();
            int Time = Int32.Parse(DateTime.Now.ToString("HH", System.Globalization.DateTimeFormatInfo.InvariantInfo));
            if (Time >= 0 && Time < 12) { GreetingLabel.Content = "Good Morning!"; }
            else if (Time >= 12 && Time < 18) { GreetingLabel.Content = "Good Afternoon!"; }
            else if (Time >= 18 && Time <= 24) { GreetingLabel.Content = "Good Evening!"; }


            Task.Delay(1);


            Dispatcher.BeginInvoke(() => RPCManager.Initialize());


            Application.Current.MainWindow = this;

        }


        private void DragWindow(object sender, MouseButtonEventArgs e) => this.DragMove();
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if(closeToTray == false) Environment.Exit(0);
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
                string authToken;
                string sheesh = e.Uri.Split("https://flarial.net/?code=")[1];
                string rawToken = await Task.Run(() => Auth.postReq(sheesh));


                var atd = JsonConvert.DeserializeObject<AccessTokenData>(rawToken);


                authToken = atd.access_token;
                await Task.Run(() =>Auth.cacheToken(atd.access_token, DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(atd.expires_in)));


                string userResponse = await Task.Run(() => Auth.getReqUser(authToken));



                Trace.WriteLine(userResponse);
                LoginAccount(userResponse, authToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        private async Task<bool> AttemptLogin()
        {
            string authToken;


            var Cached = await Auth.getCache();

            if (Cached != null && Cached.expiry > DateTime.Now)
            {
                authToken = Cached.access_token;
                Trace.WriteLine("Cached one is valid.");
            }
            else

            {

                Trace.Write("Invalid cache");
                return false;


            }






            string userResponse = await Task.Run(() => Auth.getReqUser(authToken));



            Trace.WriteLine(userResponse);
            LoginAccount(userResponse, authToken);
            return true;

        }

        private async void LoginAccount(string userResponse, string authToken)
        {


            DiscordUser user = JsonConvert.DeserializeObject<DiscordUser>(userResponse);
            
            if (user != null)
            {
                Username.Content = user.username + "#" + user.discriminator;
                Username2.Content = user.username + "#" + user.discriminator;
                //Auth.putjoinuser(JsonConvert.DeserializeObject<AccessTokenData>(test), user.id);

                guestImage = PFP.Source;

                if (user.avatar != null) PFP.Source = new ImageSourceConverter()
                        .ConvertFromString("https://cdn.discordapp.com/avatars/"
                        + user.id + "/" + user.avatar + ".png") as ImageSource;

                if (user.avatar != null) PFP2.Source = new ImageSourceConverter()
                        .ConvertFromString("https://cdn.discordapp.com/avatars/"
                        + user.id + "/" + user.avatar + ".png") as ImageSource;

            }

            string guildUserContent = await Task.Run(() => Auth.getReqGuildUser(authToken));
            Trace.WriteLine(guildUserContent);
            
            DiscordGuildUser guildUser = JsonConvert.DeserializeObject<DiscordGuildUser>(guildUserContent);

            
                if (guildUser.roles.Contains("1059408198261551145"))
                {
                    ifBeta = true;
                    BetaDLLButton.Visibility = Visibility.Visible;
                    BetaTag.Visibility = Visibility.Visible;
                    Trace.WriteLine("iz beta bro");
                }
                else
                {
                    Trace.WriteLine("No no no NOT BETA BRO!");
                }
            
            
            

            w.Close();

            isLoggedIn = true;
            LoginButton.Visibility = Visibility.Hidden;
            LogoutButton.Visibility = Visibility.Visible;

            LoginGrid.Visibility = Visibility.Hidden;



            MainGrid.Visibility = Visibility.Visible;



            //   BackupManager.createBackup("Safety");
            //  BackupManager.loadBackup("Safety");

        }



        private async void Inject_Click(object sender, RoutedEventArgs e)
        {
            FileInfo fi = new FileInfo(custom_dll_path);

            if(custom_dll_path == "amongus")
            {
                WebClient webClient = new WebClient();

                DownloadProgressChangedEventHandler among =
                    new DownloadProgressChangedEventHandler(DownloadProgressCallback);
                webClient.DownloadProgressChanged += among;
                await webClient.DownloadFileTaskAsync(new Uri("https://horion.download/dll"), "Among.dll");
                await Injector.Inject($"{Managers.VersionManagement.launcherPath}\\Among.dll", statusLabel);
            }
            else
            {
                if (File.Exists(custom_dll_path) && fi.Extension == ".dll")
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
            if(closeToTray == false) Environment.Exit(0);
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
            if(custom_dll_path == "amongus")
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
            BetaDLLButton.Visibility = Visibility.Hidden;
            isLoggedIn = false;
        }

        private void LoginAmongus(object sender, RoutedEventArgs e)
        {
            LoginGrid.Visibility = Visibility.Visible;
            MainGrid.Visibility = Visibility.Hidden;
            OptionsGrid.Visibility = Visibility.Hidden;
        }

        private async void SaveConfig(object sender, RoutedEventArgs e)
        {

            await Config.saveConfig(minecraft_version, custom_dll_path, shouldUseBetaDLL, closeToTray);

        }

        private void Window_OnClosing(object? sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CustomDialogBox MessageBox = new CustomDialogBox("Why do we need this?", "Because why tf not, stop being a fucking pussy and give us your discord token.", "MessageBox");
            MessageBox.ShowDialog();
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
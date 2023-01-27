using Flarial.Launcher.Functions;
using Flarial.Launcher.Managers;
using Flarial.Launcher.Utils;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Flarial.Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static readonly HttpClient client = new HttpClient();
        public Window1 w = new Window1();
        public MainWindow()
        {
            Environment.CurrentDirectory = Managers.VersionManagement.launcherPath;
            InitializeComponent();
            Directory.CreateDirectory(Managers.VersionManagement.launcherPath);
            int Time = Int32.Parse(DateTime.Now.ToString("HH", System.Globalization.DateTimeFormatInfo.InvariantInfo));
            if (Time >= 0 && Time < 12) { GreetingLabel.Content = "Good Morning!"; }
            else if (Time >= 12 && Time < 18) { GreetingLabel.Content = "Good Afternoon!"; }
            else if (Time >= 18 && Time <= 24) { GreetingLabel.Content = "Good Evening!"; }




        }

        private void DragWindow(object sender, MouseButtonEventArgs e) => this.DragMove();
        private void CloseWindow(object sender, RoutedEventArgs e) => this.Close();
        private void HideGrid(object sender, RoutedEventArgs e)
        {
            MainGrid.Visibility = Visibility.Visible;
            LoginGrid.Visibility = Visibility.Hidden;
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
                string rawToken = Auth.postReq(e.Uri.Split("https://flarial.net/?code=")[1]);


                var atd = JsonConvert.DeserializeObject<AccessTokenData>(rawToken);


                authToken = atd.access_token;
                await Auth.cacheToken(atd.access_token, DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(atd.expires_in));


                string userResponse = Auth.getReqUser(authToken);



                Trace.WriteLine(userResponse);
                await LoginAccount(userResponse);
                return true;
            }
            catch (Exception ex)
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

                return false;


            }






            string userResponse = Auth.getReqUser(authToken);



            Trace.WriteLine(userResponse);
            await LoginAccount(userResponse);
            return true;

        }

        private async Task LoginAccount(string userResponse)
        {


            DiscordUser user = JsonConvert.DeserializeObject<DiscordUser>(userResponse);
            Username.Content = user.username + "#" + user.discriminator;
            //Auth.putjoinuser(JsonConvert.DeserializeObject<AccessTokenData>(test), user.id);


            if (user.avatar != null) PFP.Source = new ImageSourceConverter()
                    .ConvertFromString("https://cdn.discordapp.com/avatars/"
                    + user.id + "/" + user.avatar + ".png") as ImageSource;

            Trace.WriteLine("https://cdn.discordapp.com/avatars/" + user.id + "/" + user.avatar + ".png");


            MainGrid.Visibility = Visibility.Visible;
            LoginGrid.Visibility = Visibility.Hidden;
            w.Close();

            Minecraft.Init();
            if (!Directory.Exists(BackupManager.backupDirectory)) { Directory.CreateDirectory(BackupManager.backupDirectory); }
            //   BackupManager.createBackup("Safety");
            BackupManager.loadBackup("Safety");

        }



        private async void Inject_Click(object sender, RoutedEventArgs e)
        {

            await Injector.Inject("OnixClient.dll", statusLabel);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}
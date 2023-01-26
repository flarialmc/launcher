using Flarial.Launcher.Functions;
using Flarial.Launcher.Utils;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
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
            InitializeComponent();

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

            w.Show();

            await w.web.EnsureCoreWebView2Async();
            w.web.CoreWebView2.Navigate("https://discord.com/api/oauth2/authorize?client_id=1058426966602174474&redirect_uri=https%3A%2F%2Fflarial.net%2F&response_type=code&scope=guilds%20identify%20guilds.members.read%20guilds.join");
            w.web.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
        }

        private void CoreWebView2_NavigationStarting(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri.StartsWith("https://flarial.net"))
            {
                Trace.WriteLine(e.Uri);
                string test = Auth.postreq(e.Uri.Split("https://flarial.net/?code=")[1]);
                Trace.WriteLine(test);


                string authcode = JsonConvert.DeserializeObject<AccessTokenData>(test).access_token;

                string test2 = Auth.getrequser(authcode);
                Trace.WriteLine(test2);
                DiscordUser user = JsonConvert.DeserializeObject<DiscordUser>(test2);
                Username.Content = user.username + "#" + user.discriminator;
                //Auth.putjoinuser(JsonConvert.DeserializeObject<AccessTokenData>(test), user.id);
                if (user.avatar != null) PFP.Source = new ImageSourceConverter().ConvertFromString("https://cdn.discordapp.com/avatars/" + user.id + "/" + user.avatar + ".png") as ImageSource;
                Trace.WriteLine("https://cdn.discordapp.com/avatars/" + user.id + "/" + user.avatar + ".png");
                MainGrid.Visibility = Visibility.Visible;
                LoginGrid.Visibility = Visibility.Hidden;
                w.Close();
            }
        }


    }
}
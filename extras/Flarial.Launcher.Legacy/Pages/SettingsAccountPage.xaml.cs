using Flarial.Launcher.Functions;
using Flarial.Launcher.Structures;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Flarial.Launcher.Pages;

/// <summary>
/// Interaction logic for SettingsAccountPage.xaml
/// </summary>
public partial class SettingsAccountPage : Page
{
    public Window1 w = new Window1();

    public SettingsAccountPage()
    {
        InitializeComponent();
        var settings = Settings.Current;
        var autoLogin = settings.AutoLogin;
        if (autoLogin) Dispatcher.InvokeAsync(AttemptLogin);
    }

    private async Task loginner()
    {
        var result = await AttemptLogin();
        if (result) { return; }

        w.WindowTitle = "https://discord.com/api/oauth2/authorize?client_id=1058426966602174474&response_type=code&redirect_uri=https%3A%2F%2Fflarial.net&scope=guilds.members.read+identify+guilds";

        w.Show();
        w.web.UseLayoutRounding = true;
        await w.web.EnsureCoreWebView2Async();

        w.web.CoreWebView2.Navigate("https://discord.com/api/oauth2/authorize?client_id=1058426966602174474&response_type=code&redirect_uri=https%3A%2F%2Fflarial.net&scope=guilds.members.read+identify+guilds");


        w.web.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;

    }

    private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {

    }

    private async void CoreWebView2_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
    {
        Trace.WriteLine(e.Uri.ToString());
        if (e.Uri.Contains("https://flarial.net"))
        {
            Trace.WriteLine("Found");
            await AttemptLoginWithoutCache(e);
        }
    }

    private async Task<bool> AttemptLoginWithoutCache(Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
    {
        try
        {
            if (Uri.TryCreate(e.Uri, UriKind.Absolute, out Uri uri) && uri != null)
            {
                string code = uri.Query.TrimStart('?').Split('&').FirstOrDefault(p => p.StartsWith("code="))?.Substring(5);
                if (!string.IsNullOrEmpty(code))
                {
                    string rawToken = Auth.postReq(code);
                    var atd = JsonConvert.DeserializeObject<AccessTokenData>(rawToken);
                    await Auth.CacheToken(atd.access_token, DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(atd.expires_in));
                    string userResponse = await Auth.getReqUser(atd.access_token);
                    await LoginAccount(userResponse, atd.access_token);
                    return true;
                }
            }
        }
        catch (Exception trolling)
        {
            Trace.WriteLine(trolling.StackTrace);
        }
        return false;
    }

    private async Task<bool> AttemptLogin()
    {

        var cached = await Auth.GetCache();
        if (cached != null && cached.expiry > DateTime.Now)
        {
            string userResponse = await Auth.getReqUser(cached.access_token);
            await LoginAccount(userResponse, cached.access_token);
            return true;
        }
        return false;
    }

    private async Task LoginAccount(string userResponse, string authToken)
    {
        Trace.WriteLine("TRYING TO LOG IN!");
        DiscordUser user = JsonConvert.DeserializeObject<DiscordUser>(userResponse);
        if (user != null)
        {
            MainWindow.Username.Text = user.username;
            //Username2.Content = user.username;
            //guestImage = PFP.Source;

            if (user.avatar != null)
            {
                var imageSource = new BitmapImage(new Uri($"https://cdn.discordapp.com/avatars/{user.id}/{user.avatar}.png"));
                Image.ImageSource = imageSource;
                MainWindow.PFP.ImageSource = imageSource;
                RoleLabel.Text = user.username;
                NameLabel.Text = "Logged in as:";
            }
        }

        string guildUserContent = await Auth.getReqGuildUser(authToken);
        DiscordGuildUser guildUser = JsonConvert.DeserializeObject<DiscordGuildUser>(guildUserContent);
        if (guildUser.roles != null)
        {
            if (guildUser.roles.Contains("1050447423635460197") ||
                guildUser.roles.Contains("1058465209443958816") ||
                guildUser.roles.Contains("1059109828267606066") ||
                guildUser.roles.Contains("1059332938774364160") ||
                guildUser.roles.Contains("1268949825865650268") ||
                guildUser.roles.Contains("1058465626689118280") ||
                guildUser.roles.Contains("1059086166378422352"))
            {
                MainWindow.isPremium = true;
                //ifBeta = true;
                /*BetaDLLButton.Visibility = Visibility.Visible;
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
                else if (guildUser.roles.Contains("1050447423635460197"))
                    ExecTag.Visibility = Visibility.Visible;*/
                Trace.WriteLine("iz beta bro");
            }
            else
            {
                Trace.WriteLine("No no no NOT BETA BRO!");
            }
        }

        w.Close();
        MainWindow.isLoggedIn = true;
        //LoginButton.Visibility = Visibility.Hidden;
        //LogoutButton.Visibility = Visibility.Visible;
        //LoginGrid.Visibility = Visibility.Hidden;
        //MainGrid.Visibility = Visibility.Visible;
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        await loginner();
    }
}

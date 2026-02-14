using DiscordRPC;
using DiscordRPC.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using Flarial.Launcher.Functions;
using Flarial.Launcher.Services.Game;

namespace Flarial.Launcher.Managers;

public static class RPCManager
{
    private static DiscordRpcClient client;
    private static string _discordTime = "";
    private static string previousContent = "";
    private static Timer _timer;

    public static async Task Initialize()
    {
        await Task.Run(() =>
        {
            InitializeDiscordClient();
        });

        _discordTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        _timer = new Timer(100); // 100 milliseconds
        _timer.Elapsed += TimerElapsed;
        _timer.Enabled = true;

        InLauncher();
    }

    private static void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        if (Minecraft.Installed && Minecraft.Current.Running)
        {
            var ip = readIp();
            if (ip != previousContent)
            {

                var a = GetServerInfo(ip);
                previousContent = ip; // Add this line to update previousContent
                if (a.largeImageKey == "flarialbig")
                {
                    a.largeImageKey = "mcicon";
                }

                SetPresence(a.Detail, a.largeImageKey, "flarialbig", a.ipAddress);
            }
        }
        else
        {
            InLauncher();
        }
    }


    public static string readIp()
    {
        var flarialPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Packages",
            "Microsoft.MinecraftUWP_8wekyb3d8bbwe",
            "RoamingState",
            "Flarial",
            "serverip.txt"
        );

        if (File.Exists(flarialPath))
            return File.ReadAllText(flarialPath);

        return "";
    }

    public static void InLauncher()
    {
        SetPresence("In Launcher", "None", "flarialbig", "Flarial Launcher");
    }

    private static void InitializeDiscordClient()
    {
        client = new DiscordRpcClient("1058426966602174474");
        client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
        client.OnReady += (sender, e) => Trace.WriteLine("Received Ready from user {0}", e.User.Username);
        client.OnPresenceUpdate += (sender, e) => Trace.WriteLine("Received Update! {0}", e.Presence.ToString());
        client.Initialize();
    }

    private static RichPresence currentPresence = null;

    private static void SetPresence(string details, string smallKey, string bigKey, string bigimgText)
    {
        DateTime? dateTimestampEnd = null;
        if (!string.IsNullOrEmpty(_discordTime) && int.TryParse(_discordTime, out int timestampEnd))
        {
            dateTimestampEnd = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestampEnd);
        }

        var assets = new Assets
        {
            LargeImageKey = bigKey,
            LargeImageText = bigimgText,
            SmallImageKey = smallKey != "None" ? smallKey : null
        };

        if (currentPresence == null)
        {
            // Add the "Download" button
            var button = new Button
            {
                Label = "Download",
                Url = "https://flarial.xyz/discord"

            };

            currentPresence = new RichPresence();
            currentPresence.Timestamps = new Timestamps
            {
                Start = _discordTime != "" && int.TryParse(_discordTime, out int timestampStart)
                    ? new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestampStart)
                    : DateTime.UtcNow,
                End = dateTimestampEnd
            };

            currentPresence.Buttons = new Button[] { button };
        }

        currentPresence.Details = details;
        currentPresence.Assets = assets;

        client.SetPresence(currentPresence);
    }

    private static serverInformation GetServerInfo(string ip)
    {
        switch (ip)
        {
            case string _ when ip.Contains("hive"):
                return new serverInformation()
                {
                    ipAddress = ip,
                    largeImageKey = "hivemc",
                    Detail = "Hive Network"
                };
            case string _ when ip.Contains("nethergames"):
                return new serverInformation()
                {
                    ipAddress = ip,
                    largeImageKey = "ngmc",
                    Detail = "Nethergames Network"
                };
            case string _ when ip.Contains("cubecraft"):
                return new serverInformation()
                {
                    ipAddress = ip,
                    largeImageKey = "ccmc",
                    Detail = "Cubecraft Network"
                };
            case string _ when ip.Contains("zeqa"):
                return new serverInformation()
                {
                    ipAddress = ip,
                    largeImageKey = "zeqamc",
                    Detail = "Zeqa Network"
                };
            case "none":
                return new serverInformation()
                {
                    ipAddress = ip,
                    largeImageKey = "flarialbig",
                    Detail = "Ready to play"
                };
            case "none ":
                return new serverInformation()
                {
                    ipAddress = ip,
                    largeImageKey = "flarialbig",
                    Detail = "Ready to play"
                };
            case "world":
                return new serverInformation()
                {
                    ipAddress = "In a world",
                    largeImageKey = "flarialbig",
                    Detail = "Playing Singleplayer"
                };
            case "world ":
                return new serverInformation()
                {
                    ipAddress = "In a world",
                    largeImageKey = "flarialbig",
                    Detail = "Playing Singleplayer"
                };
            default:
                return new serverInformation()
                {
                    ipAddress = ip,
                    Detail = "Playing on " + ip,
                    largeImageKey = "flarialbig"
                };
        }
    }

    public class serverInformation
    {
        public string ipAddress { get; set; }
        public string largeImageKey { get; set; }
        public string Detail { get; set; }
    }
}

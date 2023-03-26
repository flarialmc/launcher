using DiscordRPC;
using DiscordRPC.Logging;
using System;
using System.Threading.Tasks;

namespace Flarial.Launcher.Managers
{
    public static class RPCManager
    {
        public static DiscordRpcClient client;
        private static string _discordTime = "";

        public static async Task Initialize()
        {
            dynamic dateTimestampEnd = null;
            
            await Task.Run( () =>
            {

                if (_discordTime != "" && int.TryParse(_discordTime, out int timestampEnd))
                    dateTimestampEnd = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestampEnd);
                /*
                Create a Discord client
                NOTE: 	If you are using Unity3D, you must use the full constructor and define
                         the pipe connection.
                */
                client = new DiscordRpcClient("1067854754518151168");

                //Set the logger
                client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

                //Subscribe to events
                client.OnReady += (sender, e) =>
                {
                    Console.WriteLine("Received Ready from user {0}", e.User.Username);
                };

                client.OnPresenceUpdate += (sender, e) => { Console.WriteLine("Received Update! {0}", e.Presence); };

                //Connect to the RPC
                client.Initialize();

                //Set the rich presence
                //Call this as many times as you want and anywhere in your code.
                client.SetPresence(new RichPresence()
                {
                    Details = "In Launcher",
                    //        State = "csharp example",
                    Assets = new Assets()
                    {
                        LargeImageKey = "flarialbig",
                        LargeImageText = "Flarial Launcher",
                        //       SmallImageKey = "image_small"
                    },
                    Timestamps = new Timestamps
                    {
                        Start = _discordTime != "" && int.TryParse(_discordTime, out int timestampStart)
                            ? new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                                .AddSeconds(timestampStart)
                            : DateTime.UtcNow,
                        End = dateTimestampEnd
                    }
                });

            });
        }
        public static async Task ResetPresence()
        {
            dynamic dateTimestampEnd = null;

            await Task.Run(() =>
            {
                if (_discordTime != "" && int.TryParse(_discordTime, out int timestampEnd))
                    dateTimestampEnd = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestampEnd);

                client.SetPresence(new DiscordRPC.RichPresence
                {
                    Details = "Ready to play",

                    Assets = new Assets
                    {
                        LargeImageKey = "flarialbig",
                        LargeImageText = "Flarial Launcher"
                    },
                    Timestamps = new Timestamps
                    {
                        Start = _discordTime != "" && int.TryParse(_discordTime, out int timestampStart)
                            ? new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                                .AddSeconds(timestampStart)
                            : DateTime.UtcNow,
                        End = dateTimestampEnd
                    }
                });
            });
        }
    }
}
